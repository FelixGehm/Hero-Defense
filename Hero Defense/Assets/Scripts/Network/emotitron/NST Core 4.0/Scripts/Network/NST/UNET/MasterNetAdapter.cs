//Copyright 2018, Davin Carten, All rights reserved

using UnityEngine;
using UnityEngine.Networking;
using emotitron.Network.Compression;
using emotitron.Utilities.GUIUtilities;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif
namespace emotitron.Network.NST
{
	/// <summary>
	/// The UNET version of this interface for the NSTMaster - unifying code to work with both UNET and Photon.
	/// </summary>
	[DisallowMultipleComponent]
	[RequireComponent(typeof(NetworkIdentity))]
	public class MasterNetAdapter : NetworkBehaviour //, INSTMasterAdapter
	{
		NetworkIdentity NI;

		// Interfaced fields
		public NetworkLibrary NetLibrary { get { return NetworkLibrary.UNET; } }
		public static bool ServerIsActive { get { return NetworkServer.active; } }
		public static bool ClientIsActive { get { return NetworkClient.active; } }
		public const short LowestMsgTypeId = MsgType.Highest;

		public bool IsRegistered { get { return isRegistered; } set { isRegistered = value; } }
		private bool _killMe;
		public bool KillMe { get { return _killMe; } set { _killMe = value; } }

		#region Callback Interfaces

		[HideInInspector] public static List<Component> iNstMasterEvents = new List<Component>();
		[HideInInspector] public static List<Component> iNstMasterOnStartServer = new List<Component>();
		[HideInInspector] public static List<Component> iNstMasterOnStartClient = new List<Component>();
		[HideInInspector] public static List<Component> iNstMasterOnStartLocalPlayer = new List<Component>();
		[HideInInspector] public static List<Component> iNstMasterOnNetworkDestroy = new List<Component>();

		public static void RegisterCallbackInterfaces(Component obj)
		{
			AddCallback<INstMasterEvents>(iNstMasterEvents, obj);
			AddCallback<INstMasterOnStartServer>(iNstMasterOnStartServer, obj);
			AddCallback<INstMasterOnStartClient>(iNstMasterOnStartClient, obj);
			AddCallback<INstMasterOnStartLocalPlayer>(iNstMasterOnStartLocalPlayer, obj);
			AddCallback<INstMasterOnNetworkDestroy>(iNstMasterOnNetworkDestroy, obj);
		}

		public static void UnregisterCallbackInterfaces(Component obj)
		{
			RemoveCallback<INstMasterEvents>(iNstMasterEvents, obj);
			RemoveCallback<INstMasterOnStartServer>(iNstMasterOnStartServer, obj);
			RemoveCallback<INstMasterOnStartClient>(iNstMasterOnStartClient, obj);
			RemoveCallback<INstMasterOnStartLocalPlayer>(iNstMasterOnStartLocalPlayer, obj);
			RemoveCallback<INstMasterOnNetworkDestroy>(iNstMasterOnNetworkDestroy, obj);
		}

		private static void AddCallback<T>(List<Component> list, Component obj)
		{
			if (obj is T && !list.Contains(obj))
				list.Add(obj);
		}

		private static void RemoveCallback<T>(List<Component> list, Component obj)
		{
			if (obj is T && list.Contains(obj))
				list.Remove(obj);
		}

		#endregion

		// Statics
		private static NetworkWriter writer = new NetworkWriter();
		private static short masterMsgTypeId;
		private static bool isRegistered;

		public override void OnStartServer()
		{
			RegisterHanders();

			foreach (INstMasterEvents cb in iNstMasterEvents)
				cb.OnNstMasterStartServer();

			foreach (INstMasterOnStartServer cb in iNstMasterOnStartServer)
				cb.OnNstMasterStartServer();
		}
		public override void OnStartClient()
		{
			RegisterHanders();

			foreach (INstMasterEvents cb in iNstMasterEvents)
				cb.OnNstMasterStartClient();

			foreach (INstMasterOnStartClient cb in iNstMasterOnStartClient)
				cb.OnNstMasterStartClient();
		}

		public override void OnStartLocalPlayer()
		{
			foreach (INstMasterEvents cb in iNstMasterEvents)
				cb.OnNstMasterStartLocalPlayer();

			foreach (INstMasterOnStartLocalPlayer cb in iNstMasterOnStartLocalPlayer)
				cb.OnNstMasterStartLocalPlayer();
		}

		public override void OnNetworkDestroy()
		{
			if (iNstMasterEvents != null)
				foreach (INstMasterEvents cb in iNstMasterEvents)
					cb.OnNstMasterNetworkDestroy();

			if (iNstMasterOnNetworkDestroy != null)
				foreach (INstMasterOnNetworkDestroy cb in iNstMasterOnNetworkDestroy)
					cb.OnNstMasterNetworkDestroy();
		}

		public void RegisterHanders()
		{
			if (IsRegistered)
				return;

			masterMsgTypeId = NSTSettings.EnsureExistsInScene(NSTSettings.DEFAULT_GO_NAME).masterMsgTypeId;

			if (NetworkServer.active)
				NetworkServer.RegisterHandler(masterMsgTypeId, ReceiveUpdate);

			else if (NetworkClient.active)
				NetworkManager.singleton.client.RegisterHandler(masterMsgTypeId, ReceiveUpdate);

			isRegistered = true;
		}

		private static bool updateDue;
		private static int skippedUpdates;

		public static float lastFixedUpdateTime;
		// Unity has no master network tick so we will base ours off of the fixed
		void FixedUpdate()
		{
			lastFixedUpdateTime = Time.time;

			if (skippedUpdates >= NSTSettings.single.TickEveryXFixed)
			{
				updateDue = true;
				skippedUpdates = 0;
			}

			skippedUpdates++;
		}

		private void Update()
		{

			for (int i = 0; i < NetworkSyncTransform.allNsts.Count; i++)
			{
				NetworkSyncTransform.allNsts[i].MasterCommandToInterpolate();
			}

			if (updateDue)
			{
				NSTMaster.PollAllForUpdates();
			}

			updateDue = false;
		}

		/// <summary>
		///  Updates over the network arrive here - AFTER the Update() runs (not tested for all platforms... thanks unet for the great docs.) 
		///  The incoming bitstream is read
		/// </summary>
		/// <param name="msg"></param>
		private static void ReceiveUpdate(NetworkMessage msg)
		{
			UdpBitStream bitstream = new UdpBitStream(msg.reader.ReadBytesNonAlloc(NSTMaster.bitstreamByteArray, msg.reader.Length), msg.reader.Length);
			UdpBitStream outstream = new UdpBitStream(NSTMaster.outstreamByteArray);

			NSTMaster.ReceiveUpdate(ref bitstream, ref outstream, NetworkServer.active);

			BandwidthUsage.ReportMasterBits(ref bitstream, BandwidthLogType.MasterIn);

			// Write a clone message and pass it to all the clients if this is the server receiving
			if (NetworkServer.active) // && msg.conn == nst.NI.clientAuthorityOwner)
			{
				writer.StartMessage(msg.msgType);
				//writer.WriteUncountedByteArray(bitstream.Data, msg.reader.Length);
				writer.WriteUncountedByteArray(outstream.Data, outstream.BytesUsed);
				writer.SendPayloadArrayToAllClients(msg.msgType);
			}
		}

		public void SendUpdate(ref UdpBitStream bitstream, ref UdpBitStream outstream)
		{
			// Send the bitstream to the UNET writer
			writer.StartMessage(masterMsgTypeId);
			writer.WriteUncountedByteArray(NSTMaster.bitstreamByteArray, bitstream.BytesUsed);
			writer.FinishMessage();

			// if this is the server - send to all.
			if (NetworkServer.active)
			{
				writer.SendPayloadArrayToAllClients(masterMsgTypeId, Channels.DefaultUnreliable);

				// If this is the server as client, run the ReceiveUpdate since local won't get this run.
				if (NetworkClient.active)
					NSTMaster.ReceiveUpdate(ref bitstream, ref outstream, false);
			}
			// if this is a client send to server.
			else
				NetworkManager.singleton.client.SendWriter(writer, Channels.DefaultUnreliable);
		}

		public static Transform GetPlayerSpawnPoint()
		{
			return NetworkManager.singleton.GetStartPosition();
		}

		public static GameObject GetRegisteredPlayerPrefab()
		{
			if (NetworkManager.singleton == null)
				NetworkManager.singleton = FindObjectOfType<NetworkManager>();

			if (NetworkManager.singleton != null)
			{
				return NetworkManager.singleton.playerPrefab;
			}
			return null;
		}

		public static void Spawn(GameObject go)
		{
			NetworkServer.Spawn(go);
		}


#if UNITY_EDITOR
		/// <summary>
		/// Adds a prefab with NST on it to the NetworkManager spawnable prefabs list, after doing some checks to make sure it makes sense to.
		/// Will then add as the network manager player prefab if it is set to auto spawwn and is still null.
		/// </summary>
		public static void AddAsRegisteredPrefab(GameObject go, bool silence = false)
		{
			PrefabType type = PrefabUtility.GetPrefabType(go);

			NetworkManager NM = UnetBitstreamSerializers.GetNetworkManagerSingleton();

			if (type == PrefabType.None)
			{
				if (!silence)
					Debug.LogWarning("You have a NST component on a gameobject '" + go.name + "', which is not a prefab. Be sure to make '" + go.name + "' a prefab, otherwise it cannot be registered with the NetworkManager for network spawning.");
			}
			else
			{
				// Try to get a prefab source from Assets
				GameObject prefabGO = (type == PrefabType.PrefabInstance) ? go.GetPrefabSourceOfGameObject() :
					(type == PrefabType.Prefab) ? go 
					: null;

				if (prefabGO == null)
					return;

				if (!NM.spawnPrefabs.Contains(prefabGO))
				{
					Debug.LogWarning("Automatically adding '" + prefabGO.name + "' to the NetworkManager spawn list");
					NM.spawnPrefabs.Add(prefabGO);
					Debug.LogWarning(NM.spawnPrefabs[NM.spawnPrefabs.Count - 1]);
				}

				// Add the calling gameObject as the player prefab if the network manager still needs one.
				if (NM.playerPrefab == null && NM.autoCreatePlayer)
				{
					Debug.LogWarning("Automatically adding '" + prefabGO.name + "' to the NetworkManager as the Player Prefab. If this isn't desired, assign your the correct prefab to the Network Manager, or turn off Auto Create Player in the NetworkManager.");
					NM.playerPrefab = prefabGO;
					go.GetComponent<NetworkIdentity>().localPlayerAuthority = true;
				}
			}
		}

#endif

	}

#if UNITY_EDITOR

	[CustomEditor(typeof(MasterNetAdapter))]
	[CanEditMultipleObjects]
	public class MasterNetAdapterEditor : NSTHeaderEditorBase
	{
		NetworkIdentity ni;

		public override void OnEnable()
		{
			headerColor = HeaderSettingsColor;
			headerName = HeaderMasterName;
			base.OnEnable();
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			MasterNetAdapter _target = (MasterNetAdapter)target;

			// make sure the network identiy has the correct settings as often as possible.
			ni = _target.GetComponent<NetworkIdentity>();
			ni.localPlayerAuthority = false;

			// kill the network adapter if it is the wrong one (user changed netlib selection)
			if (_target.KillMe && Event.current.type == EventType.Repaint)
			{
				DestroyImmediate(_target);
				return;
			}

			base.OnInspectorGUI();
			EditorGUILayout.HelpBox("This is the Adapter for UNET. To work with Photon (PUN) replace with the Photon Adapter.", MessageType.None);
		}
	}

#endif
}
