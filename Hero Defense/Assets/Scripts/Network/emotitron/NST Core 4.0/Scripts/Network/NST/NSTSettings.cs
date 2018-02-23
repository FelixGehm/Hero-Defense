//Copyright 2018, Davin Carten, All rights reserved

using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.Networking;
using emotitron.Utilities.GUIUtilities;

namespace emotitron.Network.NST
{

	public enum NetworkLibrary { UNET, Photon }

	[HelpURL("https://docs.google.com/document/d/1nPWGC_2xa6t4f9P0sI7wAe4osrg4UP0n_9BVYnr5dkQ/edit#heading=h.fq7c7pcliv4e")]
	[AddComponentMenu("NST/NST Settings")]
	[System.Serializable]
	public class NSTSettings : Singleton<NSTSettings>
	{
		public const string DEFAULT_GO_NAME = "NST Settings";

		#region Inspector Items

		public NetworkLibrary networkingLibrary;

		[Header("Header Sizes")]

		[Space]
		
		[BitsPerRange(1,32, false, "Max NST Objects:")]
		[Tooltip("Set this to the smallest number that works for your project. 1 bit = 2 NST object max, 4 bits = 16 NST objects max, 5 bits = 32 NST objects max, 32 bits = Unlimited")]
		public int bitsForNstId = 6;

		[HideInInspector]
		public uint MaxNSTObjects;

		[Space]
		[Tooltip("Dictates the number of frames in the circular buffer. Lowering this from 6 bits/64 frames increases the risk of not having enough buffer size, and objects will begin to lurch in odd ways.")]
		[BitsPerRange(4, 6, false, "Frame Count:")]
		public int bitsForPacketCount = 6;

		[Tooltip("The Master network tick rate in relation to the the FixedUpdate")]
		[Range(1, 5)]
		public int TickEveryXFixed = 3;

		[HideInInspector]
		public int frameCount;

		[Space]

		[Tooltip("The Unet MsgType short value for NST Updates.")]
		[Range(MasterNetAdapter.LowestMsgTypeId, short.MaxValue)]
		public short masterMsgTypeId = 6000;

		[Header("World Vector Compression")]
		[Space]

		[Range(10, 1000)] [Tooltip("Indicate the minimum resolution of any axis of compressed root positions (Subdivisions per 1 Unit). Increasing this needlessly will increase your network traffic. Decreasing it too much will result in objects moving in visible rounded increments.")]
		public int minPosResolution = 100;

		[Tooltip("If no NSTMapBounds are found in the scene, this is the size of the world that will be used by the root position compression engine.")]
		public Bounds defaultWorldBounds = new Bounds(new Vector3(0,0,0), new Vector3(2000,100,2000));

		[Header("Debugging")]
		[Help("All log options are for editor only and will be conditionally purged from all builds. No need to disable these for releases.")]

		[Tooltip("Turn this off for your production build to reduce pointless cpu waste.")]
		public bool logWarnings = true;

		[Tooltip("Spam your log with all kinds of info you may or may not care about. Turn this off for your production build to reduce pointless cpu waste.")]
		public bool logTestingInfo = false;

		[Tooltip("Put itemized summaries of update bandwidth usage into the Debug.Log")]
		public bool logDataUse = false;

		#endregion

		protected override void Awake()
		{
			// Base does singleton enforcement
			base.Awake();

			NSTMaster.EnsureExistsInScene("NST Master");
			// TODO Add items like this to an idiot check test bool so they can be disabled.
			DestroyAllNSTsInScene();

			// Tell DebugX what the logging choices were.
			DebugX.logInfo = logTestingInfo;
			DebugX.logWarnings = logWarnings;
			DebugX.logErrors = true;

			//Calculate the max objects at the current bits for NstId
			MaxNSTObjects = (uint)Mathf.Pow(2, bitsForNstId);

			frameCount =
				(bitsForPacketCount == 4) ? 16 :
				(bitsForPacketCount == 5) ? 32 :
				(bitsForPacketCount == 6) ? 64 :
				0; // zero will break things, but it should never happen so breaking it would be good.
		}


		private void Start()
		{

			// Destroy any NSTs the developer may have left in the scene.
			List<NetworkSyncTransform> nsts = FindObjects.FindObjectsOfTypeAllInScene<NetworkSyncTransform>(true);
			for (int i = 0; i < nsts.Count; i++)
			{
				Destroy(nsts[i].gameObject);
			}

			// Not ideal code to prevent hitching issues with vsync being off - ensures a reasonable framerate is being enforced
			if (QualitySettings.vSyncCount == 0)
			{
				if (Application.targetFrameRate <= 0)
					Application.targetFrameRate = 60;
				else
					Application.targetFrameRate = Application.targetFrameRate;

				DebugX.LogWarning(!DebugX.logWarnings ? "" : 
					("VSync appears to be disabled, which can cause some problems with Networking. \nEnforcing the current framerate of " + Application.targetFrameRate + 
					" to prevent hitching. Enable VSync or set 'Application.targetFrameRate' as desired if this is not the framerate you would like."));
			}
		}

		public static void DestroyAllNSTsInScene()
		{
			NetworkSyncTransform[] litter = FindObjectsOfType<NetworkSyncTransform>();
			for (int i = 0; i < litter.Length; i++)
				Destroy(litter[i].gameObject);
		}
	}
}
