//Copyright 2018, Davin Carten, All rights reserved

using UnityEngine;
//using UnityEngine.Networking;
using emotitron.Network.Compression;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif
namespace emotitron.Network.NST
{

	//[NetworkSettings(sendInterval = 0)]
	[DisallowMultipleComponent]
	public class NSTMaster : Singleton<NSTMaster>
	{
		public static MasterNetAdapter mna;
		public const string defGameObjectName = "NST Master";

		// the num of bits used to count how many bytes an update used - this is a bit arbitrary
		internal const int UPDATELENGTH_BYTE_COUNT_SIZE = 6;

		// Preallocated reusable byte arrays for the bitstream and outstream
		internal static byte[] bitstreamByteArray = new byte[1024];
		internal static byte[] outstreamByteArray = new byte[1024];

		protected override void Awake()
		{
			base.Awake();
			mna = GetComponent<MasterNetAdapter>();
			//EnforceSingleton();
		}

		private void Start()
		{
			//mna = EnsureHasCorrectAdapter();
		}

		public void Reset()
		{
			//EnforceSingleton();
			mna = EnsureHasCorrectAdapter();
		}

		//private void EnforceSingleton()
		//{
		//	// Base does singleton enforcement
		//	if (single != null && single != this)
		//	{
		//		Debug.LogWarning("Enforcing NSTMaster singleton. Multiples found.");
		//		Destroy(this);
		//	}
		//	single = this; ;
		//}

		//TODO add library selection logic here for selecting correct adapter
		public MasterNetAdapter EnsureHasCorrectAdapter()
		{

			NSTSettings nstSettings = NSTSettings.EnsureExistsInScene(NSTSettings.DEFAULT_GO_NAME);

			mna = GetComponent<MasterNetAdapter>();
			if (!mna)
				mna = gameObject.AddComponent<MasterNetAdapter>();

			if (mna && mna.NetLibrary != nstSettings.networkingLibrary)
			{
				Debug.LogWarning("Photon PUN not yet supported");
				nstSettings.networkingLibrary = NetworkLibrary.UNET;
			}



			////if (Event.current.type == EventType.Repaint)
			//if (mna != null && mna.NetLibrary != networkingLibrary)
			//	mna.KillMe = true;


			//if (mna == null)
			//{
			//	// Hacky way to try and apply classes that may or may not exist.
			//	Type type = (networkingLibrary == NetworkLibrary.Photon) ?
			//		Type.GetType("emotitron.Network.NST.NSTMasterPhotonAdapter") :
			//		Type.GetType("emotitron.Network.NST.NSTMasterUnetAdapter");

			//	if (type != null)
			//		gameObject.AddComponent(type);

			//	mna = GetComponent<INSTMasterAdapter>();
			//}


			// Should exist now... try again.
			return mna;

		}

		// NOTE Update fires BEFORE network messages are received. Not sure if this is always true though for unet on all platforms.
		private void Update()
		{
			// Test for  a shutdown condition
			if (!MasterNetAdapter.ClientIsActive && !MasterNetAdapter.ServerIsActive && mna.IsRegistered) // !NetworkClient.active && !NetworkServer.active && isRegistered)
			{
				mna.IsRegistered = false;
				return;
			}
			
			// Test for a network restart condition
			else if (MasterNetAdapter.ClientIsActive || MasterNetAdapter.ServerIsActive)
			{
				mna.RegisterHanders();
			}

			//for (int i = 0; i < NetworkSyncTransform.allNsts.Count; i++)
			//	NetworkSyncTransform.allNsts[i].MasterPollForFrameUpdates();

			// Poll for network changes after update() replacement has run on each NST
			//PollAllForUpdates();
			
		}

		private void LateUpdate()
		{
			for (int i = 0; i < NetworkSyncTransform.allNsts.Count; i++)
				NetworkSyncTransform.allNsts[i].MasterLateUpdate();
		}

		/// <summary>
		/// Ping all owned NSTs for any due updates. Passes the bitstream to that NST to write to.
		/// </summary>
		public static void PollAllForUpdates()
		{
			//Debug.Log("<b><color=green>Mstr Ping</color></b> " + (Time.time - tempMasterPollTime));
			//tempMasterPollTime = Time.time;

			UdpBitStream bitstream = new UdpBitStream(bitstreamByteArray);
			UdpBitStream outstream = new UdpBitStream();

			bool foundUpdate = false;
			for (int i = 0; i < NetworkSyncTransform.allNsts.Count; i++)
			{
				foundUpdate |= NetworkSyncTransform.allNsts[i].PollForUpdate(ref bitstream);
			}

			// No Updates, we are done.
			if (!foundUpdate)
				return;

			// Write the end of stream marker of 00
			bitstream.WriteBool(false);

			mna.SendUpdate(ref bitstream, ref outstream);

			//Debug.Log("<b><color=blue>Mstr Snd</color></b> " + (Time.time - tempMasterSendTime));
			//tempMasterSendTime = Time.time;
		}
		static float tempMasterPollTime;

		/// <summary>
		/// Reads update headers for each NST frame update in the incoming bitstream, and passes the bitstream to that NST to read out its
		/// update information.
		/// </summary>
		/// <param name="mirror">True if this is the server, and this is the incoming bitstream. Tells the server that the outstream
		/// needs to be populated for retransmission to all clients. Also false if this is the server running its own outgoing update.</param>
		public static void ReceiveUpdate(ref UdpBitStream bitstream, ref UdpBitStream outstream, bool mirror)
		{
			
			// Create a new bitstream to ensure ptr is at 0. Same data as master though.
			bitstream.Ptr = 0;
			
			// remove this safety once working
			//TEST
			int safety = 0;
			UpdateType updateType;
			do
			{
				safety++;
				BandwidthUsage.Start(ref bitstream, BandwidthLogType.UpdateRcv);

				//stop looking when header is EOS
				bool notEOS = bitstream.ReadBool();
				int mirrorUpdateStartPtr = outstream.Ptr;
				BandwidthUsage.AddUsage(ref bitstream, "NotEOS");

				if (mirror)
					outstream.WriteBool(notEOS);

				if (!notEOS)
					break;

				// First three bits are the msgtype 
				//TODO this might only need to be two
				updateType = (UpdateType)bitstream.ReadInt(3);
				BandwidthUsage.AddUsage(ref bitstream, "UpdateType");

				int updateBitstreamPos = outstream.Ptr;
				if (mirror)
					outstream.WriteInt((int)updateType, 3);

				// Next variable is the NstId - get it to know where to send the rest of the bitstream
				uint nstid = bitstream.ReadUInt(NSTSettings.single.bitsForNstId);
				BandwidthUsage.AddUsage(ref bitstream, "NstId");

				if (mirror)
					outstream.WriteUInt(nstid, NSTSettings.single.bitsForNstId);

				NetworkSyncTransform nst = NetworkSyncTransform.GetNstFromId(nstid);
				BandwidthUsage.SetName(nst);

				int updatelength = bitstream.ReadInt(UPDATELENGTH_BYTE_COUNT_SIZE);
				if (mirror) outstream.WriteInt(updatelength, UPDATELENGTH_BYTE_COUNT_SIZE);
				BandwidthUsage.AddUsage(ref bitstream, "DataLength");

				//Note the starting pos in stream
				int bodyPtr = bitstream.Ptr;
				// The start pos for modifying update lenght for mirror
				int mirrorBodyPtr = outstream.Ptr;

				// This mising NST handler is NOT FULLY TESTED. Uses the updatelength value to jump ahead in the bitstream if the NST it is
				// addressed to doesn't exist for some reason.
				if (nst == null)
				{
					DebugX.LogWarning(!DebugX.logWarnings ? "" : ("Message for an NST Object arrived but that object does not exist."));
					// Forward to the next update start in the incoming stream.
					bitstream.Ptr = bodyPtr + (updatelength << 3);
					// rewind to the EOS marker and pretend this arrival never occured for the outgoing mirror stream.
					outstream.Ptr = mirrorUpdateStartPtr;
					continue;
				}

				// Tell this nst to read its mail. updateType may get modified by server receive for things like teleport.
				updateType = nst.ReceieveGeneric(ref bitstream, ref outstream, updateType, updatelength, mirror);

				// overwrite the updateType of the server outgoing in case it has changed.
				if (mirror)
					outstream.WriteIntAtPos((int)updateType, 3, updateBitstreamPos);

				//Advance ptr to next update in stream by force, in case the last update wasn't read for any reason (such as the NST leaving the game)
				bitstream.Ptr = bodyPtr + (updatelength << 3);
				
				// write the update byte length for the mirror (not the same value as the incoming due to server side adjustments)
				if (mirror)
				{
					int holdPos = outstream.Ptr;
					outstream.Ptr = mirrorBodyPtr - UPDATELENGTH_BYTE_COUNT_SIZE;
					// get the bytesused rounded up.
					int bytes = ((holdPos - mirrorBodyPtr) >> 3) + (((holdPos - mirrorBodyPtr) % 8 == 0 ) ? 0 : 1 );
					outstream.WriteInt(bytes, UPDATELENGTH_BYTE_COUNT_SIZE);
					outstream.Ptr = mirrorBodyPtr + (bytes << 3);
				}

			} while (safety < 100);
		}
	}

#if UNITY_EDITOR

	[CustomEditor(typeof(NSTMaster))]
	[CanEditMultipleObjects]
	public class NSTMasterEditor : NSTHeaderEditorBase
	{
		NSTMaster nstMaster;
		MasterNetAdapter mna;
		NSTSettings nstSettings;

		public override void OnEnable()
		{
			nstMaster = (NSTMaster)target;
			mna = nstMaster.EnsureHasCorrectAdapter();

			headerName = HeaderMasterName;
			headerColor = HeaderSettingsColor;
			base.OnEnable();
		}

		public override void OnInspectorGUI()
		{
			nstMaster = (NSTMaster)target;
			mna = nstMaster.EnsureHasCorrectAdapter();
			nstSettings = NSTSettings.EnsureExistsInScene(NSTSettings.DEFAULT_GO_NAME);

			base.OnInspectorGUI();

			if (mna == null)
				EditorGUILayout.HelpBox(
					"No network library adapter found for '" + 
					Enum.GetName(typeof(NetworkLibrary), nstSettings.networkingLibrary) + "'", MessageType.Error);

			EditorGUILayout.HelpBox("The NST Master is a required engine singleton. It collects and dispatches all NST Updates, and receives incoming updates from the network.", MessageType.None);
			
		}
	}
#endif
}

