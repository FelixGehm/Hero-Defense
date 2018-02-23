using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using emotitron.Network.Compression;
using emotitron.Utilities.GUIUtilities;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace emotitron.Network.NST
{
	// Callbacks that the NST uses for notifications of network events
	public interface INstMasterEvents
	{
		void OnNstMasterStartServer();
		void OnNstMasterStartClient();
		void OnNstMasterStartLocalPlayer();
		void OnNstMasterNetworkDestroy();
	}

	public interface INstMasterOnStartServer
	{
		void OnNstMasterStartServer();
	}
	public interface INstMasterOnStartClient
	{
		void OnNstMasterStartClient();
	}
	public interface INstMasterOnStartLocalPlayer
	{
		void OnNstMasterStartLocalPlayer();
	}
	public interface INstMasterOnNetworkDestroy
	{
		void OnNstMasterNetworkDestroy();
	}

	//public interface IAdapterEventListen
	//{
	//	List<Action> StartServerCallbacks { get; }
	//	List<Action> StartClientCallbacks { get; }
	//	List<Action> StartStartLocalPlayerCallbacks { get; }
	//	List<Action> NetworkDestroyCallbacks { get; }
	//}

	/// <summary>
	/// This class contains the abstracted methods for different networking libraries. 
	/// This adapter is for UNET.
	/// </summary>
	[DisallowMultipleComponent]
	[NetworkSettings(sendInterval = 0)]
	public class NSTNetAdapter : NetworkBehaviour //, INstNetAdapter
	{
		NetworkIdentity NI;
		NetworkSyncTransform nst;
		NSTSettings nstSettings;

		[HideInInspector] public List<INstMasterEvents> iNstMasterEvents = new List<INstMasterEvents>();
		[HideInInspector] public List<INstMasterOnStartServer> iNstMasterOnStartServer = new List<INstMasterOnStartServer>();
		[HideInInspector] public List<INstMasterOnStartClient> iNstMasterOnStartClient = new List<INstMasterOnStartClient>();
		[HideInInspector] public List<INstMasterOnStartLocalPlayer> iNstMasterOnStartLocalPlayer = new List<INstMasterOnStartLocalPlayer>();
		[HideInInspector] public List<INstMasterOnNetworkDestroy> iNstMasterOnNetworkDestroy = new List<INstMasterOnNetworkDestroy>();

		public bool IsServer { get { return isServer; } }
		public bool IsLocalPlayer { get { return isLocalPlayer; } }
		public bool HasAuthority { get { return hasAuthority; } }
		
		public uint NetId { get { return NI.netId.Value; } }

		[SyncVar]
		private uint _nstIdSyncvar;
		public uint NstIdSyncvar { get { return _nstIdSyncvar; } set { _nstIdSyncvar = value; } }

		public void CollectCallbackInterfaces()
		{
			GetComponentsInChildren(true, iNstMasterEvents);
		}

		void Awake()
		{
			NI = GetComponent<NetworkIdentity>();

			CollectCallbackInterfaces();
		}
	
		public override void OnStartServer()
		{
			foreach (INstMasterEvents cb in iNstMasterEvents)
				cb.OnNstMasterStartServer();

			foreach (INstMasterOnStartServer cb in iNstMasterOnStartServer)
				cb.OnNstMasterStartServer();
		}
		public override void OnStartClient()
		{
			// Be super sure the HLAPI didn't take a dump and set the nstID on clients if in unlimited mode.
			if (NSTSettings.EnsureExistsInScene(NSTSettings.DEFAULT_GO_NAME).bitsForNstId == 32)// MaxNstObjects.Unlimited)
				NstIdSyncvar = NI.netId.Value;

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

		/// <summary>
		/// Get the RTT in seconds for the owner of this network object. Only valid on Server.
		/// </summary>
		public float GetRTT()
		{
			NetworkConnection conn = NI.clientAuthorityOwner;
			byte error = 0;
			return (conn == null || conn.hostId == -1) ? 0 :
				.001f * NetworkTransport.GetCurrentRTT(NI.clientAuthorityOwner.hostId, NI.clientAuthorityOwner.connectionId, out error);
		}
		
		/// <summary>
		/// Get the RTT to the player who owns this NST
		/// </summary>
		public static float GetRTT(NetworkSyncTransform nstOfOwner)
		{
			return nstOfOwner.na.GetRTT();
		}

		public void SendBitstreamToOwner(ref UdpBitStream bitstream)
		{
			NI.clientAuthorityOwner.SendBitstreamToThisConn(ref bitstream, Channels.DefaultUnreliable);
		}

		/// <summary>
		/// Remove the adapter and the NetworkIdentity/View from an object
		/// </summary>
		/// <param name="nst"></param>
		public static void RemoveAdapter(NetworkSyncTransform nst)
		{
			NetworkIdentity ni = nst.GetComponent<NetworkIdentity>();
			NSTNetAdapter na = nst.GetComponent<NSTNetAdapter>();

			if (na)
				DestroyImmediate(na);

			if (ni)
				DestroyImmediate(ni);
		}

#if UNITY_EDITOR

		/// <summary>
		/// Add a network adapter and the NetworkIdenity/NetworkView as needed
		/// </summary>
		public static NSTNetAdapter EnsureNstAdapterExists(GameObject go)
		{
			go.transform.root.gameObject.EnsureRootComponentExists<NetworkIdentity>();
			NSTNetAdapter na = go.transform.root.gameObject.EnsureRootComponentExists<NSTNetAdapter>(false);

			return na;
		}

#endif
	}

#if UNITY_EDITOR

	[CustomEditor(typeof(NSTNetAdapter))]
	[CanEditMultipleObjects]
	public class NSTNetAdapterEditor : NSTHeaderEditorBase
	{
		public override void OnEnable()
		{
			headerName = HeaderAnimatorAddonName;
			headerColor = HeaderAnimatorAddonColor;
			base.OnEnable();
		}
	}
#endif



}
