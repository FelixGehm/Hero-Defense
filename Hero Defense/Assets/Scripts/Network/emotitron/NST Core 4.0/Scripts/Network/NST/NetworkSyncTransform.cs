//Copyright 2018, Davin Carten, All rights reserved

using System;
using UnityEngine;
using System.Collections.Generic;
using emotitron.Utilities.BitUtilities;
using emotitron.Utilities.SmartVars;
using emotitron.Network.Compression;

namespace emotitron.Network.NST
{

	[AddComponentMenu("NST/Network Sync Transform")]
	[DisallowMultipleComponent]
	[HelpURL("https://docs.google.com/document/d/1nPWGC_2xa6t4f9P0sI7wAe4osrg4UP0n_9BVYnr5dkQ/edit#heading=h.xct6xseu49aa")]
	//[NetworkSettings(sendInterval = 0)]
	public class NetworkSyncTransform : MonoBehaviour, INSTTransformElement, INstMasterEvents
	{
		public GameObject GameObject { get { return gameObject; } }
		public TransformElement TransElement { get { return rootRotationElement; } }
		[HideInInspector] public NSTNetAdapter na;

		[SerializeField]
		private State _state = State.Alive;
		// state still being sent out by server, changes to this are lagged by half RTT to the owner

		public State State
		{
			get { return _state; }
			set
			{
				if (_state == value)
					return;

				_state = value;

				foreach (INstState cb in iNstState)
					cb.OnNstState(_state);
			}
		}

		// useable by LaggedAction()
		private void SetStateAlive() { State = State.Alive; }
		private void SetStateDead() { State = State.Dead; }

		#region Lagged Actions

		private struct LaggedAction
		{
			public float applyTime;
			public Action action;
			public LaggedAction(float applyTime, Action action) { this.applyTime = applyTime; this.action = action; }
		}
		private Queue<LaggedAction> laggedAction = new Queue<LaggedAction>();

		private void PollForDelayedActions()
		{
			while (laggedAction.Count > 0 && laggedAction.Peek().applyTime < Time.time)
			{
				DebugX.Log("Found lagged action " + laggedAction.Peek());
				laggedAction.Dequeue().action.Invoke();
			}
		}

		/// <summary>
		/// Server Only. Will execute an action in Half the Round Trip Time between the owner and server.
		/// </summary>
		private void QueueLaggedAction(Action action)
		{
			float delay = Time.time + na.GetRTT() * .5f;
			laggedAction.Enqueue( new LaggedAction(delay, action));
			Debug.Log("Que action " + action + " by " + delay + " secs");
		}

		///// <summary>
		///// Get the RTT in seconds for the owner of this network object. Only valid on Server.
		///// </summary>
		//public float GetRTT()
		//{
		//	//float RTT = NetworkManager.singleton.client.GetRTT() * .001f;

		//	NetworkConnection conn = NI.clientAuthorityOwner;
		//	byte error = 0;
		//	return (conn == null || conn.hostId == -1) ? 0 :
		//		.001f * NetworkTransport.GetCurrentRTT(NI.clientAuthorityOwner.hostId, NI.clientAuthorityOwner.connectionId, out error);
		//}

		#endregion

		#region Inspector Vars

		[Range(1, 5)]
		[Tooltip("1 is the default. 1 sends updates every Network Tick (set in NSTSettings). 3 every 3rd.")]
		public int sendEveryXTick = 1;

		[Range(0f, .5f)]
		[Tooltip("Target number of milliseconds to buffer. Higher creates more induced latency, lower smoother handling of network jitter and loss.")]
		public float desiredBufferMS = .1f;

		[Range(0f, 1f)]
		[Tooltip("How aggressively to try and maintain the desired buffer size.")]
		public float bufferDriftCorrectAmt = .1f;

		[Tooltip("Let NST guess the best settings for isKinematic and interpolation for your rigidbodies on server/client/localplayer. Turn this off if you want to set them yourself in your own code.")]
		[SerializeField]
		private bool autoKinematic = true;

		[Tooltip("Offitck allows 'AddCustomEventToQueue()' calls to be sent out from owners immediately to reduce latency, but will increase data usage. " +
			"Also, note that since these immediate updates aren't buffered or interpolated, positions and rotations involved may differ from the rendered world.")]
		public bool allowOfftick = true;

		[Header("Root Position Updates")]

		[Utilities.GUIUtilities.EnumMask]
		public SendCullMask sendOnEvent = SendCullMask.OnChanges | SendCullMask.OnTeleport | SendCullMask.OnCustomMsg | SendCullMask.OnRewindCast;

		[XYZSwitchMask]
		public IncludedAxes includedAxes = (IncludedAxes)7;

		[Range(0f, 16f)]
		[Tooltip("How often to force a position keyframe. These ensure that with network errors or newly joined players objects will not remain out of sync long.")]
		public int keyEvery = 5;

		[Tooltip("0 = No extrapolation. 1 = Full extrapolation. Extrapolation occurs when the buffer runs out of frames. Without extrapolation the object will freeze if no new position updates have arrived in time. With extrapolation the object will continue in the direction it was heading as of the last update until a new update arrives.")]
		[Range(0, 1)]
		public float extrapolation = .5f;
		[HideInInspector] public int extrapolationDivisor; // cached invert of extrapolate (1 / extrapolate) for int math

		[Tooltip("The max number of sequential frames that will be extrapolated. Too large a value and objects will wander too far during network hangs, too few and network objects will freeze when the buffer empties. Extrapolation should not be occurring often - if at all, so a smaller number is ideal (default = 1 frame).")]
		[Range(0,4)]
		public int maxExtrapolates = 2;

		[Tooltip("A change in postion greater than this distance in units will treat the move as a teleport. This means the object will move to that location without any tweening.")]
		[SerializeField]
		private float teleportThreshold = 1;

		[Header("Root Position Upper Bit Culling")]
		[Tooltip("Enabling this reduces position data by not sending the higher order bits of the compressed positions unless they have changed. This can greatly reduce data usage on larger maps (such as a battlefield), and is not recommended for small maps (such as Pong). It does introduce the possibility of odd behavior from bad connections though.")]
		public bool cullUpperBits = false;

		[Tooltip("When using upper bit culling, this value dictates how many full frames in a row will be sent after upper bits have changed. The higher this number the lower the risk of lost packets creating mayhem. Too high and you will end up with nothing but keyframes.")]
		[SerializeField]
		[Range(1, 10)]
		private int sequentialKeys = 5;

		[Space]
		public RotationElement rootRotationElement = new RotationElement() { isRoot = true, useLocal = false, name = "Root Rotation" };

		[Header("Debuging")]
		[SerializeField]
		public DebugXform debugXform;
		[HideInInspector] public GameObject debugXformGO;

		#endregion

		#region NstId Syncvar and methods

		// There are two storage vehicles. The dictionary is used for Unlimited and the NST[] for smaller numbers (currently 5 bits / 32 objects)
		private static Dictionary<uint, NetworkSyncTransform> nstIdToNSTLookup = new Dictionary<uint, NetworkSyncTransform>();
		private static NetworkSyncTransform[] NstIds;

		public static NetworkSyncTransform localPlayerNST;
		public static List<NetworkSyncTransform> allNsts = new List<NetworkSyncTransform>();

		// Public methods for looking up game objects by the NstID
		public static NetworkSyncTransform GetNstFromId(uint id)
		{
#if UNITY_EDITOR 
			// this test won't be needed at runtime since NSTSettings will already be up and running
			NSTSettings.EnsureExistsInScene(NSTSettings.DEFAULT_GO_NAME);
#endif
			// 5 bits (32 objects) is the arbitrary cutoff point for using an array for the lookup. For greater numbers the dictionary is used instead.
			if (NSTSettings.single.bitsForNstId > 5)
				return nstIdToNSTLookup[id];
			else
				return NstIds[(int)id];
		}

		// Save the last new dictionary opening found to avoid retrying to same ones over and over when finding new free keys.
		private static int nstDictLastCheckedPtr;
		private static int GetFreeNstId()
		{
			if (NSTSettings.single.bitsForNstId < 6)
			{
				for (int i = 0; i < NstIds.Length; i++)
				{
					if (NstIds[i] == null)
						return i;
				}
			}
			else
			{
				for (int i = 0; i < 64; i++)
				{
					int offseti = (int)((i + nstDictLastCheckedPtr + 1) % NSTSettings.single.MaxNSTObjects);
					if (nstIdToNSTLookup[(uint)offseti] == null)
					{
						nstDictLastCheckedPtr = offseti;
						return offseti;
					}
				}
			}

			Debug.LogError("No more available NST ids. Increase the number Max Nst Objects in NST Settings, or your game will be VERY broken.");
			return -1;
		}

		// The network ID actually is synced using the network adaptor, since that is network library dependent.
		public uint NstId
		{
			// TODO remove this null check once adapter fully worked out
			get { return (na != null) ? na.NstIdSyncvar : 0; }
			private set { na.NstIdSyncvar = value; }
		}

		private Frame CurrentFrame
		{
			get { return buffer.currentFrame; }
			set { buffer.currentFrame = value; }
		}

		private int CurrentIndex
		{
			get { return buffer.currentFrame.frameid; }
		}
		private Frame OfftickFrame;

		#endregion

		#region Callback Interfaces

		[HideInInspector] public List<INstState> iNstState = new List<INstState>();
		[HideInInspector] public List<INstAwake> iNstAwake = new List<INstAwake>();
		[HideInInspector] public List<INstPreInterpolate> iNstPreInterpolate = new List<INstPreInterpolate>();
		[HideInInspector] public List<INstPostInterpolate> iNstPostInterpolate = new List<INstPostInterpolate>();
		[HideInInspector] public List<INstPreLateUpdate> iNstPreLateUpdate = new List<INstPreLateUpdate>();
		[HideInInspector] public List<INstPostLateUpdate> iNstPostLateUpdate = new List<INstPostLateUpdate>();
		[HideInInspector] public List<INstStart> iNstStart = new List<INstStart>();
		[HideInInspector] public List<INstOnStartServer> iNstOnStartServer = new List<INstOnStartServer>();
		[HideInInspector] public List<INstOnStartClient> iNstOnStartClient = new List<INstOnStartClient>();
		[HideInInspector] public List<INstOnStartLocalPlayer> iNstOnStartLocalPlayer = new List<INstOnStartLocalPlayer>();
		[HideInInspector] public List<INstOnNetworkDestroy> iNstOnNetworkDestroy = new List<INstOnNetworkDestroy>();
		[HideInInspector] public List<INstOnDestroy> iNstOnDestroy = new List<INstOnDestroy>();
		[HideInInspector] public List<INstBitstreamInjectFirst> iBitstreamInjects = new List<INstBitstreamInjectFirst>();
		[HideInInspector] public List<INstBitstreamInjectSecond> iBitstreamInjectSecond = new List<INstBitstreamInjectSecond>();
		[HideInInspector] public List<INstBitstreamInjectThird> iBitstreamInjectsLate = new List<INstBitstreamInjectThird>();
		[HideInInspector] public List<INstGenerateUpdateType> iGenerateUpdateType = new List<INstGenerateUpdateType>();
		[HideInInspector] public List<INstOnExtrapolate> iNstOnExtrapolate = new List<INstOnExtrapolate>();
		[HideInInspector] public List<INstOnReconstructMissing> iNstOnReconstructMissing = new List<INstOnReconstructMissing>();
		[HideInInspector] public List<INstOnSndUpdate> iNstOnSndUpdate = new List<INstOnSndUpdate>();
		[HideInInspector] public List<INstOnRcvUpdate> iNstOnRcvUpdate = new List<INstOnRcvUpdate>();
		[HideInInspector] public List<INstOnOwnerIncomingRootPos> iNstOnOwnerIncomingRoot = new List<INstOnOwnerIncomingRootPos>();
		[HideInInspector] public List<INstOnSvrOutgoingRootPos> iNstOnSvrOutgoingRoot = new List<INstOnSvrOutgoingRootPos>();
		[HideInInspector] public List<INstOnSnapshotToRewind> iNstOnSnapshotToRewind = new List<INstOnSnapshotToRewind>();
		[HideInInspector] public List<INstOnStartInterpolate> iNstOnStartInterpolate = new List<INstOnStartInterpolate>();
		[HideInInspector] public List<INstOnEndInterpolate> iNstOnEndInterpolate = new List<INstOnEndInterpolate>();
		[HideInInspector] public List<INstOnInterpolate> iNstOnInterpolate = new List<INstOnInterpolate>();
		[HideInInspector] public List<INstOnSvrInterpolateRoot> iNstOnSvrInterpRoot = new List<INstOnSvrInterpolateRoot>();
		[HideInInspector] public List<INstPreFixedUpdate> iNstPreFixedUpdate = new List<INstPreFixedUpdate>();
		[HideInInspector] public List<INstPostFixedUpdate> iNstPostFixedUpdate = new List<INstPostFixedUpdate>();
		[HideInInspector] public List<INstTeleportIncoming> iNstTeleportIncoming = new List<INstTeleportIncoming>();
		[HideInInspector] public List<INstOnTeleportApply> iNstOnTeleportApply = new List<INstOnTeleportApply>();

		public void CollectCallbackInterfaces()
		{
			// Collect all interfaces
			// TODO rebuild this to be one Find call, and then sort all of the results
			GetComponentsInChildren(true, iNstState);
			GetComponentsInChildren(true, iNstAwake);
			GetComponentsInChildren(true, iNstPreInterpolate);
			GetComponentsInChildren(true, iNstPostInterpolate);
			GetComponentsInChildren(true, iNstPreLateUpdate);
			GetComponentsInChildren(true, iNstPostLateUpdate);
			GetComponentsInChildren(true, iNstStart);
			GetComponentsInChildren(true, iNstOnStartServer);
			GetComponentsInChildren(true, iNstOnStartClient);
			GetComponentsInChildren(true, iNstOnStartLocalPlayer);
			GetComponentsInChildren(true, iNstOnNetworkDestroy);
			GetComponentsInChildren(true, iNstOnDestroy);
			GetComponentsInChildren(true, iBitstreamInjects);
			GetComponentsInChildren(true, iBitstreamInjectSecond);
			GetComponentsInChildren(true, iBitstreamInjectsLate);
			GetComponentsInChildren(true, iGenerateUpdateType);
			GetComponentsInChildren(true, iNstOnExtrapolate);
			GetComponentsInChildren(true, iNstOnReconstructMissing);
			GetComponentsInChildren(true, iNstOnSndUpdate);
			GetComponentsInChildren(true, iNstOnRcvUpdate);
			GetComponentsInChildren(true, iNstOnOwnerIncomingRoot);
			GetComponentsInChildren(true, iNstOnSvrOutgoingRoot);
			GetComponentsInChildren(true, iNstOnSnapshotToRewind);
			GetComponentsInChildren(true, iNstOnStartInterpolate);
			GetComponentsInChildren(true, iNstOnInterpolate);
			GetComponentsInChildren(true, iNstOnEndInterpolate);
			GetComponentsInChildren(true, iNstOnSvrInterpRoot);
			GetComponentsInChildren(true, iNstPreFixedUpdate);
			GetComponentsInChildren(true, iNstPostFixedUpdate);
			GetComponentsInChildren(true, iNstTeleportIncoming);
			GetComponentsInChildren(true, iNstOnTeleportApply);
		}

		#endregion

		#region Startup and Initialization

		// Cached Components
		[HideInInspector] public Rigidbody rb;
		[HideInInspector] public NSTElementsEngine nstElementsEngine;
		//[HideInInspector] public NetworkIdentity NI;
		[HideInInspector] public NSTSettings nstSettings;

		public virtual void Awake()
		{
			na = GetComponent<NSTNetAdapter>();

			if (na == null)
				DebugX.LogError("No Network Library adapter found for " + name);

			// this is an unspawned NST object in the scene at start, and will be deleted.
			if (!MasterNetAdapter.ServerIsActive && !MasterNetAdapter.ClientIsActive)
			{
				Destroy(transform.root.gameObject);
				return;
			}

			// Ensure core Singletons exist in case the developer completely missed this step somehow.
			nstSettings = NSTSettings.EnsureExistsInScene(NSTSettings.DEFAULT_GO_NAME);
			nstElementsEngine = NSTElementsEngine.EnsureExistsOnRoot(transform, false);

			// Cache components
			rb = GetComponent<Rigidbody>();
			
			CollectCallbackInterfaces();

			// determine the update interval based on the current physics clock rate.
			frameUpdateInterval = Time.fixedDeltaTime * sendEveryXTick * nstSettings.TickEveryXFixed;
			invFrameUpdateInterval = 1f / frameUpdateInterval;

			// Don't allow target buffer size to be smaller than the frameUpdateInterval, or else nudge starts to cause constant resyncs trying to acheive the impossibly small.
			desiredBufferMS = Mathf.Max(desiredBufferMS, frameUpdateInterval);

			extrapolationDivisor = (int)(1f / extrapolation);

			foreach (INstAwake cb in iNstAwake)
				cb.OnNstPostAwake();
		}

		public void OnNstMasterStartServer()
		{
			Initialize();

			foreach (INstOnStartServer cb in iNstOnStartServer)
				cb.OnNstStartServer();
		}

		public void OnNstMasterStartClient()
		{
			if (!na.IsServer)
				Initialize();

			foreach (INstOnStartClient cb in iNstOnStartClient)
				cb.OnNstStartClient();
		}


		private void Initialize()
		{
			// Be sure that the elements count exists before doing the buffer init.
			nstElementsEngine.Initialize();

			// Moved this from awake to init to give elements time to initialize element[0] (root rotation)
			buffer = new FrameBuffer(this, nstElementsEngine, nstSettings.frameCount, transform.position, transform.rotation);

			OfftickFrame = buffer.frames[0];
			// If the nstid array is null - it need to be created. Leave the nst array null for unlimited - that uses the dictionary.
			if (NstIds == null && nstSettings.bitsForNstId < 6)
				NstIds = new NetworkSyncTransform[nstSettings.MaxNSTObjects];

			// Server needs to set the syncvar for the NstId
			if (na.IsServer)
			{
				if (NSTSettings.single.bitsForNstId < 6)
					na.NstIdSyncvar = (uint)GetFreeNstId();
				else
					na.NstIdSyncvar = na.NetId;
			}

			if (NSTSettings.single.bitsForNstId < 6)
				NstIds[na.NstIdSyncvar] = this;

			else if (!nstIdToNSTLookup.ContainsKey(na.NstIdSyncvar))
				nstIdToNSTLookup.Add(na.NstIdSyncvar, this);

			allNsts.Add(this);

			// Create a DebugWidget for this NST
			debugXformGO = DebugWidget.CreateDebugCross();
		}

		public void OnNstMasterStartLocalPlayer()
		{
			localPlayerNST = this;

			foreach (INstOnStartLocalPlayer cb in iNstOnStartLocalPlayer)
				cb.OnNstStartLocalPlayer();
		}

		public virtual void Start()
		{
			ApplyTeleportLocally(OfftickFrame);

			// Automatically determine what the kinematic and interpolations settings should be
			if (autoKinematic && rb != null)
			{
				// if the prefab rb isn't set to isKinematic, set to to kinematic for dumb clients. Leave it as is for owner
				// We are assuming how it is set is how the developer wanted it to be for the owner.
				if (!rb.isKinematic)
					rb.isKinematic = (!na.HasAuthority && !MasterNetAdapter.ServerIsActive);

				rb.interpolation = (!na.HasAuthority || rb.isKinematic) ? RigidbodyInterpolation.None : RigidbodyInterpolation.Interpolate;
			}

			foreach (INstStart cb in iNstStart)
				cb.OnNstStart();
		}

		public virtual void OnDestroy()
		{
			Shutdown();

			if (iNstOnDestroy != null)
				foreach (INstOnDestroy cb in iNstOnDestroy)
					cb.OnNstDestroy();
		}

		public void OnNstMasterNetworkDestroy()
		{
			Shutdown();

			if (iNstOnNetworkDestroy != null)
				foreach (INstOnNetworkDestroy cb in iNstOnNetworkDestroy)
					cb.OnNstNetworkDestroy();
		}

		private void Shutdown()
		{
			if (na != null && nstIdToNSTLookup != null && nstIdToNSTLookup.ContainsKey(na.NetId))
				nstIdToNSTLookup.Remove(na.NetId);

			if (na != null && NstIds != null)
				NstIds[na.NstIdSyncvar] = null;

			if (allNsts != null && allNsts.Contains(this))
				allNsts.Remove(this);

			if (debugXformGO != null)
				Destroy(debugXformGO);
		}

		#endregion

		#region Updates

		// Local Player sends its transform updates based on the fixed update
		void FixedUpdate()
		{
			int mostRecentRcvdFrameId = (mostRecentRcvdFrame != null) ? mostRecentRcvdFrame.frameid : -1;

			foreach (INstPreFixedUpdate callback in iNstPreFixedUpdate)
				callback.OnNstPreFixedUpdate(this, FrameCount, mostRecentRcvdFrameId);

			foreach (INstPostFixedUpdate callback in iNstPostFixedUpdate)
				callback.OnNstPostFixedUpdate(this, FrameCount, mostRecentRcvdFrameId);
		}

		/// <summary>
		/// Called when the NSTMaster passes along its Update() to owned NST objects.
		/// </summary>
		public void MasterCommandToInterpolate()
		{
			// TODO not sure the best timing for this.
			PollForDelayedActions();

			foreach (INstPreInterpolate cb in iNstPreInterpolate)
				cb.OnNstPreInterpolate();

			if (!na.HasAuthority)
				InterpolateTransform();

			foreach (INstPostInterpolate cb in iNstPostInterpolate)
				cb.OnNstPostInterpolate();
		}

		public void MasterLateUpdate()
		{
			foreach (INstPreLateUpdate cb in iNstPreLateUpdate)
				cb.OnNstPreLateUpdate();

			foreach (INstPostLateUpdate cb in iNstPostLateUpdate)
				cb.OnNstPostLateUpdate();
		}

		private int skippedUpdates = 1;

		public bool PollForUpdate(ref UdpBitStream bitstream)
		{
			if (na.HasAuthority)
			{

				if (skippedUpdates >= sendEveryXTick)
				{
					GenerateUpdate(ref bitstream);
					//DebugText.Log(skippedUpdates + " " + sendEveryXTick + " " + NSTSettings.single.TickEveryXFixed);

					skippedUpdates = 1;
					return true;
				}
				skippedUpdates++;
				// if update is not due but there is an offtick item on one of the queues, send an offtick update (if NSTSettings allow for that)
				//TODO Make this nstREwind check use a public bool rather than exposing the entire queue
				if (allowOfftick && (customEventQueue.Count > 0)) // || (nstRewind != null && nstRewind.rewindCastQueue.Count > 0 )))
				{
					GenerateUpdate(ref bitstream, true);
					return true;
				}
			}
			// no update this time
			return false;
		}

		#endregion

		#region Interpolation

		public FrameBuffer buffer;

		// calculated at startup based on number of skipped frames
		[HideInInspector] public float frameUpdateInterval;
		[HideInInspector] public float invFrameUpdateInterval;
		private bool waitingForFirstFrame = true;

		// interpolation vars
		private Vector3 posSnapshot;
		public CompressedElement lastSentCompPos, lastSentCompPosKey;
		[HideInInspector] public Vector3 lastSentPos;
		private float lastSentFixedTime;

		float nudge = 0;

		private Vector3 targetVelocity;
		private Vector3 expectedPosNextUpdate;

		private void InterpolateTransform()
		{
			// If we need a new Frame (lerped to end of last one or waiting at start)
			if (Time.time >= CurrentFrame.endTime)
			{
				// Still no Frame has arrived - nothing to do yet
				if (waitingForFirstFrame == true && CurrentIndex == 0 && buffer.validFrameMask <= 1)
				{
					DebugX.Log(!DebugX.logInfo ? "" : 
						(Time.time + " " + name + " <color=black>Still waiting for first frame update." + "</color>" + "  buffer..." + buffer.validFrameMask));

					CurrentFrame.endTime = Time.time;
					return;
				}

				waitingForFirstFrame = false;

				foreach(INstOnEndInterpolate cb in iNstOnEndInterpolate)
					cb.OnEndInterpolate(CurrentFrame);

				// Testing for very low framerates - play catch-up if the framerate is causing a backlog.
				int numOfFramesOverdue = (int)((Time.time - CurrentFrame.endTime) * invFrameUpdateInterval);

				if (numOfFramesOverdue > 1)
				{
					// Get the real number of frames we seem to be overdue, which is the current buffer size - the desired size.
					numOfFramesOverdue = Mathf.Max(0, (int)((buffer.CurrentBufferSize - desiredBufferMS) / frameUpdateInterval));
					CurrentFrame.endTime = Time.time;
				}
				
				// For loop is to catch up on frames if the screen update is slower than the fixed update.
				for (int overduecount = numOfFramesOverdue; overduecount >= 0; overduecount--)
				{
					//Debug.Log(
					DebugX.Log(!DebugX.logInfo ? "" :
						(Time.time + " <color=black><b>Finding Next Frame To Interpolate.</b></color> " + " NST:" + NstId + " " + name + "\nValid Frames: " + buffer.PrintBufferMask(CurrentIndex)));

					// THIS IS THE MAIN FIND SECTION WHERE NEW FRAMES ARE FOUND

					foreach (INstOnSnapshotToRewind cb in iNstOnSnapshotToRewind)
						cb.OnSnapshotToRewind(CurrentFrame);

					// Find and prepare the next frame
					Frame next = buffer.DetermineAndPrepareNextFrame(svrWaitingForOwnerTeleportConfirm);

					buffer.prevAppliedFrame = CurrentFrame;
					CurrentFrame = next;

					// Dumb Clients apply state change when it comes off of the buffer. Server dictates state. Owner applies state on update arrival.
					if (!MasterNetAdapter.ServerIsActive)// && CurrentFrame.updateType.IsTeleport())
						State = CurrentFrame.state;

					//Dumb clients apply the teleport now that is is coming off the buffer
					if (CurrentFrame.updateType.IsTeleport() && !na.IsServer)
					{
						// Teleport, but only hard if this is the first in a chain of teleports... each teleport after make soft.
						bool lastUpdatewWasAlsoTeleport = buffer.prevAppliedFrame.updateType.IsTeleport();
						ApplyTeleportLocally(CurrentFrame, !lastUpdatewWasAlsoTeleport);
					}
					
					DebugX.Log(!DebugX.logInfo ? "" : 
						(Time.time + " NST:" + NstId + " <b> Last Frame was " + buffer.prevAppliedFrame.frameid + ".  New Frame is: " + CurrentFrame.frameid + "</b> type:" + CurrentFrame.updateType + " " + 
						CurrentFrame.rootSendType +	" " + CurrentFrame.compPos + "   " + CurrentFrame.rootPos + " rot: " + CurrentFrame.RootRot));

					posSnapshot = buffer.prevAppliedFrame.rootPos;

					if (sendOnEvent != 0) //  syncRootPosition != RootPositionSync.Disabled)
					{
						// Treat a move greater than the threshold apply a soft teleport.
						if (CurrentFrame.rootSendType.IsPosType())
						{
							if ((CurrentFrame.rootPos - posSnapshot).SqrMagnitude(includedAxes) > (teleportThreshold * teleportThreshold))
							{
								DebugX.LogWarning(!DebugX.logWarnings ? "" :
									("Automatic teleport due to threshold exceeded (dist:" + Vector3.Distance(CurrentFrame.rootPos, posSnapshot) + ") - if you are seeing this and didn't expect a teleport, increase the teleport threshold on NetworkSyncTransform for object " + name), true, true);

								ApplyTeleportLocally(CurrentFrame, false);
							}
						}

						DebugWidget.Move(debugXformGO, posSnapshot, (rootRotationElement.snapshot != null) ? rootRotationElement.snapshot.RootRot : GenericX.NULL, debugXform, DebugXform.Snapshot);
						DebugWidget.Move(debugXformGO, CurrentFrame.rootPos, CurrentFrame.RootRot, debugXform, DebugXform.Uninterpolated);
					}

					foreach (INstOnStartInterpolate cb in iNstOnStartInterpolate) 
						cb.OnStartInterpolate(CurrentFrame);

					// Recalculate the buffer size (This may need to happen AFTER the valid flag is set to false to be accurate) - Not super efficient doing this test too often
					buffer.UpdateBufferAverage();

					// nudge UNLESS we are running through a backlogged frames OR the buffer is empty (nudging an empty buffer will result in a lot of resyncs)
					nudge = (overduecount > 0) ? 0 :
						(desiredBufferMS - buffer.bufferAverageSize) * bufferDriftCorrectAmt;
					
					DebugX.Log(!DebugX.logInfo ? "" : 
						("nudge " + nudge + "buffer.bufferAverageSize " + buffer.bufferAverageSize + "  desiredms: " + desiredBufferMS));

					CurrentFrame.endTime = buffer.prevAppliedFrame.endTime + frameUpdateInterval + nudge;

					///Experimental code for testing Server Auth
					//// if we can use physics to move this object, push with velocity now rather than every update
					////TODO keeping this code around in case we need to make use of it for ServerAuth component.
					//if (rb != null && !rb.isKinematic)
					//{
					//	float frameduration = frameUpdateInterval + nudge;
					//	//rb.drag = 0;
					//	// Turn pos delta into vectory velocity

					//	targetVelocity = (CurrentFrame.rootSendType.IsPosType()) ?
					//		(CurrentFrame.rootPos - transform.position) / (frameduration /*+ Time.deltaTime*/) : new Vector3(0,0,0);

					//	//Debug.Log(CurrentFrame.rootSendType.IsPosType() + " " + targetVelocity.magnitude + " " + CurrentFrame.rootPos);
					//	// if below velocity threshold, set to zero to avoid oscillations
					//	//if (targetVelocity.sqrMagnitude < .1f)
					//	//	targetVelocity = new Vector3(0, 0, 0);

					//	//Debug.Log("targetVelocity " + targetVelocity + " currroot: " + CurrentFrame.rootPos + " pos: " + transform.position + " t: " + (frameduration - Time.deltaTime));
					//	//TEST force position to where it should be Time.deltatime already into next frame.
					//	//float tm = Mathf.InverseLerp(posSnapshotTime, CurrentFrame.endTime, Time.time - Time.deltaTime);
					//	//Vector3 expectedPos = gameObject.Lerp(posSnapshot, CurrentFrame.rootPos, includeXYZ, tm);
					//	////rb.MovePosition(expectedPos);
					//	//transform.position = expectedPos;

					//	// Velocity only applied on interpolate start... null if below threshold to avoid error induced oscillation
					//	//rb.velocity = targetVelocity;
					//}

					// Mark the current frame as no longer pending in the mask
					buffer.SetBitInValidFrameMask(CurrentIndex, false);
				}
			}

			// End getting next frame from buffer... now do the Lerping.

			//float t = Mathf.InverseLerp(posSnapshotTime, CurrentFrame.endTime, Time.time - Time.deltaTime);
			float t = Mathf.InverseLerp(buffer.prevAppliedFrame.endTime, CurrentFrame.endTime, Time.time);

			//TEST
			//t = 0;

			// If any root motion is possible, run the root pos interpolation.
			if (sendOnEvent != 0)
			{
				expectedPosNextUpdate = gameObject.Lerp(posSnapshot, CurrentFrame.rootPos, includedAxes, t);

				Vector3 errorForgiveAmt = new Vector3(0, 0, 0);
				
				// Apply any server interventions through this callback before applying the root lerp
				if (MasterNetAdapter.ServerIsActive && !na.HasAuthority)
					foreach (INstOnSvrInterpolateRoot cb in iNstOnSvrInterpRoot)
						errorForgiveAmt += cb.OnSvrInterpolateRoot(CurrentFrame, posSnapshot, expectedPosNextUpdate, t - Time.deltaTime);

				//Interpolate the root position
				// if there are no physics, we aren't moving this with the physics push.

				gameObject.transform.position = expectedPosNextUpdate;

				///Testing code for Server Auth tests
				//if (rb == null || rb.isKinematic)
				//{
				//	//Debug.Log(Time.time + " " + (expectedPosNextUpdate * 1000));
				//	gameObject.transform.position = expectedPosNextUpdate;
				//}
				//else
				//{

				//	// Experimental Svr Auth Code - still working this out
				//	//CurrentFrame.RootPos += rootPosErrorDelta;
				//	//rootPosErrorDelta = new Vector3(0, 0, 0);
				//	//expectedRootPositionAfterPhysics = gameObject.Lerp(posSnapshot, CurrentFrame.rootPos + rootPosErrorDelta, includeXYZ, lerpTime);

				//	// Keep correcting the velocity in case a late arrival fires between now and next frame, or server authority intervention has overriden the owners inputs.
				////	targetVelocity = (CurrentFrame.rootPos - transform.position) / ((CurrentFrame.endTime - (Time.time - Time.deltaTime)));

				//	// if below velocity threshold, set to zero to avoid oscillations
				//	//if (targetVelocity.sqrMagnitude < .1f)
				//	//	targetVelocity = new Vector3(0, 0, 0);

				//	// Velocity applied every update to hopefully counteract drag?
				//	//rb.velocity = targetVelocity + errorForgiveAmt;
				//	//rb.MovePosition(expectedPosNextUpdate);
				//}
			}

			if (rootRotationElement.target.RootRot.type == XType.NULL)
				Debug.Log(Time.time + " target Snap now NULL:" + rootRotationElement.target.RootRot);

			foreach (INstOnInterpolate cb in iNstOnInterpolate)
				cb.OnInterpolate(t);
		}

		#endregion

		#region Custom Events

		private Queue<byte[]> customEventQueue = new Queue<byte[]>();

		/// <summary>
		/// This static method assumes you have ONLY ONE NST on this client. If you have more than one use the non-static AddCustomEventToQueue method to specify which NST you mean. 
		/// Tack your own data on the end of the NST syncs. This can be weapon fire or any other custom action. 
		/// This will trigger the OnCustomMsgSndEvent on the sending machine and OnCustomMsgRcvEvent on receiving machines.
		/// </summary>
		/// <param name="userData"></param>
		public static void SendCustomEventSimple(byte[] userData)
		{
			if (localPlayerNST != null)
				localPlayerNST.customEventQueue.Enqueue(userData);
		}

		/// <summary>
		/// This static method assumes you have ONLY ONE NST on this client. If you have more than one use the non-static AddCustomEventToQueue method to specify which NST you mean.
		/// This overlad will accept just about anything you put into a struct, so be careful. Limit your datatypes to JUST the smallest compressed primatives and don't include methods 
		/// or properties in your custom struct. Otherwise this could bloat your net traffic fast.
		/// This will trigger the OnCustomMsgSndEvent on the sending machine and OnCustomMsgRcvEvent on receiving machines.
		/// </summary>
		/// <typeparam name="T">A custom struct of your own making.</typeparam>
		public static void SendCustomEventSimple<T>(T userData) where T : struct
		{
			if (localPlayerNST != null)
				localPlayerNST.customEventQueue.Enqueue(userData.SerializeToByteArray());
		}

		/// <summary>
		/// Tack your own data on the end of the NST syncs. This can be weapon fire or any other custom action.
		/// This will trigger the OnCustomMsgSndEvent on the sending machine and OnCustomMsgRcvEvent on receiving machines.
		/// </summary>
		/// <param name="userData"></param>
		public void SendCustomEvent(byte[] userData)
		{
			customEventQueue.Enqueue(userData);
		}

		/// <summary>
		/// This overlad will accept just about anything you put into a struct, so be careful. Limit your datatypes to JUST the smallest compressed primatives and don't include methods 
		/// or properties in your custom struct. Otherwise this could bloat your net traffic fast.
		/// This will trigger the OnCustomMsgSndEvent on the sending machine and OnCustomMsgRcvEvent on receiving machines.
		/// </summary>
		/// <typeparam name="T">A custom struct of your own making.</typeparam>
		public void SendCustomEvent<T>(T userData) where T : struct
		{
			customEventQueue.Enqueue(userData.SerializeToByteArray());
		}

		#endregion
		
		#region Teleport

		private bool svrWaitingForOwnerTeleportConfirm; // a teleport has occurred, next outgoing packet needs to indicate that.
		private bool ownrNeedsToSendTeleportConfirm;
		private bool dumbClientAppliedImmediateSvrTeleport;

		public void Teleport(Transform tr, bool immediate = true)
		{
			Teleport(tr.position, tr.rotation, immediate);
		}

		/// <summary>
		/// Public method for initiating a Teleport. Only can be initiated by the Server currently.
		/// </summary>
		public void Teleport(Vector3 pos, Quaternion rot, bool immediate = true)
		{
			// On the server, send the teleport command to clients, and set to confirmation waiting condition.
			// This will ignore all incoming messages from this object until a confirmation arrives.
			if (na.IsServer)
			{
				SvrCommandTeleport(pos, rot, immediate);
			}
			else
			{
				DebugX.LogWarning(!DebugX.logWarnings ? "" : ("You are trying to teleport an object from a client rather than the server. Only the server may teleport NST objects."));
				return;
			}
		}

		/// <summary>
		/// This command assumes that all elements of this networked object have been translated as desired prior to calling this.
		/// </summary>
		/// <param name="immediate"></param>
		private void SvrCommandTeleport(bool immediate = true)
		{
			svrWaitingForOwnerTeleportConfirm = true;

			if (na.HasAuthority)
				ownrNeedsToSendTeleportConfirm = true;
			
			// 'Harden' current translate of teleportOveridden elements.
			ApplyTeleportLocally(true);

			// Server owned objects need to let the teleport command happen on the next tick. Immediate breaks things.
			if (immediate)
			{
				// Teleport with SVRTPORT id indicates that this is the original teleport command - will be wanting a response teleport.
				// reuse the byte array from the NSTMaster just to be efficient
				UdpBitStream teleportBitstream = new UdpBitStream(NSTMaster.bitstreamByteArray);
				WriteGenericTransMessage(ref teleportBitstream, OfftickFrame);
				teleportBitstream.WriteBool(false);// write EOS (usually written by NSTMaster) so as to not break anything that may follow
				na.SendBitstreamToOwner(ref teleportBitstream);
			}
		}

		// Code for basic server side portion of a teleport.
		private void SvrCommandTeleport(Vector3 pos, Quaternion rot, bool immediate = true)
		{
			transform.position = pos;

			if (rootRotationElement.teleportOverride)
			{
				rootRotationElement.Teleport(rot);
			}

			SvrCommandTeleport(immediate);
		}
		/// <summary>
		/// Calls TeleportToLocation on the local NST object. This is a conveinience method and will only work reliably if you only have one NST object with local authority.
		/// </summary>
		public static void TeleportLclPlayer(Vector3 pos, Quaternion rot)
		{
			if (localPlayerNST != null)
				localPlayerNST.Teleport(pos, rot);
		}

		// Used to determine if incomming teleport commands are just the same command being repeated.
		CompressedElement lastTeleportCmdCompPos;

		/// <summary>
		/// Owner test to see if this teleport command looks like a repeat. If not, apply a hard teleport.
		/// </summary>
		private void OwnerTeleportCommandHandler(Frame frame) // Vector3 pos, Quaternion rot) //, List<GenericX> rots)
		{
			bool thisTeleportAlreadyApplied = ownrNeedsToSendTeleportConfirm && lastTeleportCmdCompPos == frame.CompRootPos;

			// Hard teleport if this looks like a new teleport
			ApplyTeleportLocally(frame, !thisTeleportAlreadyApplied);
		}

		/// <summary>
		/// Apply teleport using current position as overload
		/// </summary>
		public void ApplyTeleportLocally(bool hardTeleport = true)
		{
			OfftickFrame.CaptureCurrentTransforms();

			ApplyTeleportLocally(OfftickFrame);
		}

		/// <summary>
		/// Move root object on this client only without lerping or interpolation. Clears all buffers and snapshots. 
		/// HardTeleport will clear the buffer, soft teleport just disables the RB to avoid lerping.
		/// </summary>
		private bool ApplyTeleportLocally(Frame frame, bool hardTeleport = true)
		{
			lastTeleportCmdCompPos = frame.compPos;

			Vector3 pos = frame.compPos.DecompressFromWorld();

			//Debug.Log(
			DebugX.Log(!DebugX.logInfo ? "" :
				(Time.time + "fid:" + frame.frameid + " <color=red><b>Teleport</b></color> hard:" + hardTeleport + " new: " +
				((rb != null) ? (" rbVel:" + rb.velocity * 100f) : "" ) + " go.pos " + transform.position + "  " +
				+ frame.compPos + " / " + pos + " + Distance: " + Vector3.Distance(pos, transform.position) + "\n" + frame));
			
			bool wasKinematic = false;

			if (rb != null)
			{
				rb.Sleep();
				wasKinematic = rb.isKinematic;
				rb.velocity = new Vector3(0, 0, 0);
				rb.isKinematic = true;
			}

			// Clear ALL old frame buffer items to stop warping. They are all invalid now anyway.
			if (hardTeleport)
			{
				buffer.prevAppliedFrame.rootPos = pos;
				CurrentFrame.compPos = frame.compPos;
				CurrentFrame.rootPos = pos;
				lastRootCompPos = frame.compPos;
				lastSentCompPos = frame.compPos;

				lastSentPos = pos;
				buffer.validFrameMask = 0;
			}

			posSnapshot = pos;
			gameObject.SetPosition(pos, includedAxes); //transform.position = pos;

			// Notify applicable elements of the incoming teleport
			foreach (INstTeleportIncoming cb in iNstTeleportIncoming)
				cb.OnTeleportApply(frame);

			if (rb != null)
				rb.isKinematic = wasKinematic;

			return true;
		}

		#endregion

		#region Message Transmission

		private int _frameCount = 1;
		public int FrameCount
		{
			get { return _frameCount; }
			set
			{
				_frameCount = value;
				if (_frameCount == NSTSettings.single.frameCount) _frameCount = 1;
			}
		}

		/// Counter used to track the number of forced full position updates following an upperbits change (if uselowerbits is enabled)
		private int sequentialKeyCount = 0;
		//PLAYER WITH AUTHORITY RUNS THIS EVERY TICK
		/// <summary>Determine which msgType is needed and then call Generate using that type</summary>
		private void GenerateUpdate(ref UdpBitStream bitstream, bool isOfftick = false)
		{
				
				// Get new packetID and write to bitstream
			int packetid = isOfftick ? 0 : FrameCount++;
			Frame frame = buffer.frames[packetid];

			// for the local player, ref the current frame for other uses.
			if (na.HasAuthority)
				CurrentFrame = frame;

			if (isOfftick)
			{
				frame.compPos = transform.position.CompressToWorld();
			}
			else
			{
				/// Experimental code for removing FixedUpdate send jitter
				//Vector3 posDelta = transform.position - lastSentPos;
				//float timeSinceLastSentFixed = Time.time - lastSentFixedTime;
				//Vector3 fixedPosDelta = (posDelta / timeSinceLastSentFixed) * frameUpdateInterval;
				//lastSentPos += fixedPosDelta;
				lastSentPos += ((transform.position - lastSentPos) / (Time.time - lastSentFixedTime)) * frameUpdateInterval;
				lastSentFixedTime = Time.fixedTime;
				frame.compPos = (lastSentPos).CompressToWorld();
			}

			/// Alternate method - might accumulate errors
			//Vector3 delta = transform.position - lastSentFixedPos;
			//Vector3 correctedDelta = (delta / (Time.time - lastSentTime)) * frameUpdateInterval;
			//lastSentTime = Time.time;
			//lastSentFixedPos = transform.position;
			//frame.compPos = (lastSentFixedPos + correctedDelta).CompressToWorld();

			/// Original - no jitter removal.
			//frame.compPos = transform.position.CompressToWorld();

			bool forceKey = false;
			
			frame.updateType = 0;

			// TODO these flags can likely be moved to write now
			// If a teleport or state change has occurred, indicate that with next position update.
			if (ownrNeedsToSendTeleportConfirm)
			{
				frame.updateType |= UpdateType.Teleport;
				forceKey |= (sendOnEvent.OnTeleport());
			}

			if (customEventQueue.Count > 0)
			{
				frame.updateType |= UpdateType.Cust_Msg;
				byte[] customMsg = customEventQueue.Dequeue();
				Buffer.BlockCopy(customMsg, 0, frame.customData, 0, customMsg.Length);
				frame.customMsgSize = customMsg.Length;
				forceKey |= (sendOnEvent.OnCustomMsg() && frame.updateType.IsCustom());
			}

			// Let Rewind and such flag the update type if they have something to say.
			foreach (INstGenerateUpdateType cb in iGenerateUpdateType)
				cb.OnGenerateUpdateType(frame, ref bitstream, ref forceKey);

			frame.rootSendType = RootSendType.None;

			// TODO some of this doesn't need to happen every time. First check that a pos will be needed
			// Check for what kind of msg needs to be sent
			if (sendOnEvent != 0)  //(syncRootPosition != RootPositionSync.Disabled)
			{
				forceKey |= (keyEvery != 0) && (FrameCount % keyEvery == 0);

				bool hasMoved = (CompressedElement.Compare(frame.compPos, lastSentCompPos) == false);

				//test to make sure movement doesn't exceed keyframe limits - don't use a keyframe if to does
				if (cullUpperBits)
				{
					if (!WorldVectorCompression.TestMatchingUpper(lastSentCompPosKey, frame.compPos))
					{
						DebugX.Log(!DebugX.logInfo ? "" : (Time.time + "NST:" + NstId + " Position upper bits have changed. Sending full pos for next " + sequentialKeys + " updates"));
						sequentialKeyCount = sequentialKeys;
					}
				}
				// Brute for send x number of keyframes after an upperbit change to overcome loss problems.
				if (sequentialKeyCount != 0)
				{
					forceKey = true;
					sequentialKeyCount--;
				}

				frame.rootSendType =
					forceKey ? RootSendType.Full :
					sendOnEvent.EveryTick() ? (cullUpperBits ? RootSendType.LowerThirds : RootSendType.Full) :
					hasMoved && sendOnEvent.OnChanges() ? (cullUpperBits ? RootSendType.LowerThirds : RootSendType.Full) :
					RootSendType.None;
			}

			// Generate an update with determined msgtype
			WriteGenericTransMessage(ref bitstream, frame);

			//if (!isOfftick)
			//{
			//	frameTransmitDue = false;
			//}
		}

		/// <summary>
		/// Serialize the appropriate data the NetworkWriter.
		/// </summary>
		private void WriteGenericTransMessage(ref UdpBitStream bitstream, Frame frame)
		{
			// Update the debug transform if it is being used
			DebugWidget.Move(debugXformGO, frame.rootPos, frame.RootRot, debugXform, DebugXform.LocalSend);

			BandwidthUsage.Start(ref bitstream, BandwidthLogType.UpdateSend);
			BandwidthUsage.SetName(this);

			bitstream.WriteBool(true); // Leading bit indicates this is an update and not eos
			BandwidthUsage.AddUsage(ref bitstream, "NotEOS");

			bitstream.WriteInt((int)frame.updateType, 3);
			BandwidthUsage.AddUsage(ref bitstream, "UpdateType");

			bitstream.WriteUInt(NstId, NSTSettings.single.bitsForNstId);
			BandwidthUsage.AddUsage(ref bitstream, "NstId");

			// Store where our update length needs to be rewritten later - start counting bits used from here.
			bitstream.Ptr += NSTMaster.UPDATELENGTH_BYTE_COUNT_SIZE;
			int ptrStartPos = bitstream.Ptr;
			BandwidthUsage.AddUsage(ref bitstream, "DataLength");

			// Write frameid
			bitstream.WriteInt(frame.frameid, NSTSettings.single.bitsForPacketCount);
			BandwidthUsage.AddUsage(ref bitstream, "Frame Id");

			bitstream.WriteInt((int)State, 2);

			BandwidthUsage.AddUsage(ref bitstream, "State");

			//Write rootPos send type
			bitstream.WriteInt((int)frame.rootSendType, 2);
			BandwidthUsage.AddUsage(ref bitstream, "Root Pos Comp Type");

			// Send position key or lowerbits
			if (frame.rootSendType.IsPosType())
			{
				frame.compPos.WriteWorldCompPosToBitstream(ref bitstream, includedAxes, frame.rootSendType.IsLBits());

				// Write the position to own frame for server auth functions later.
				frame.rootPos = frame.compPos.DecompressFromWorld();

				lastSentCompPos = frame.compPos;
				lastSentPos = frame.rootPos;

				// If this is a key, log it - send compressed pos is used to determine if upperbits have changed in future updates.
				if (frame.frameid != 0 && frame.rootSendType.IsPosType())
				{
					lastSentCompPosKey = frame.compPos;
				}
			}
			BandwidthUsage.AddUsage(ref bitstream, "Root Pos");

			// Inject Elements
			foreach (INstBitstreamInjectFirst cb in iBitstreamInjects)
				cb.NSTBitstreamOutgoingFirst(frame, ref bitstream);
			BandwidthUsage.AddUsage(ref bitstream, "Elements");

			// Inject Animator (and any other server independent pass through elements)
			foreach (INstBitstreamInjectSecond cb in iBitstreamInjectSecond)
				cb.NSTBitstreamOutgoingSecond(frame, ref bitstream);
			BandwidthUsage.AddUsage(ref bitstream, "Animator");

			// Inject RewindCasts
			foreach (INstBitstreamInjectThird cb in iBitstreamInjectsLate)
				cb.NSTBitstreamOutgoingThird(frame, ref bitstream);
			BandwidthUsage.AddUsage(ref bitstream, "Casts");

			// Inject user data
			if (frame.updateType.IsCustom())
			{
				bitstream.WriteByteArray(frame.customData, frame.customMsgSize);
			}
			BandwidthUsage.AddUsage(ref bitstream, "User Custom Data");

			// Notify all interfaces that we are about to send a frame update, Snapshots are applied on this timing.
			foreach (INstOnSndUpdate cb in iNstOnSndUpdate)
				cb.OnSnd(frame);

			// Determine the length written rounded up, and determine the padding needed to make this an even byte
			int bitWritten = bitstream.Ptr - ptrStartPos;
			int evenBytes = (bitWritten >> 3) + ((bitWritten % 8 == 0) ? 0 : 1);
			int padding = (evenBytes << 3) - bitWritten;

			// jump back and rewrite the size now that we know it. Then forward to where we were, plus the padding needed to make this even bytes.
			bitstream.WriteIntAtPos(evenBytes, NSTMaster.UPDATELENGTH_BYTE_COUNT_SIZE, ptrStartPos - NSTMaster.UPDATELENGTH_BYTE_COUNT_SIZE);
			bitstream.Ptr += padding; // advance the ptr to make this an even number of bytes

			BandwidthUsage.AddUsage(ref bitstream, "Padding");
			BandwidthUsage.PrintSummary();

			DebugX.Log(!DebugX.logInfo ? "" :
				(Time.time + " NST:" + NstId + " <b>Sent </b>" + frame));

		}

		#endregion

		#region Message Reception

		//TODO initialize this at start if this works
		private CompressedElement lastRootCompPos;

		private Frame mostRecentRcvdFrame;

		/// <summary>
		/// Universal receiver for incoming frame updates. Both server and clients run through this same method. If it is the server 'asServer' will be true.
		/// Frames updates are read in from the master bitstream, and parsed to their appropriately numbered frame in the frame buffer. Frames with frameid of zero
		/// are flagged as 'immediate' - they are not added to the buffer but rather are used to pass through offtick updates such as weapon and teleport commands.
		/// </summary>
		/// <param name="bitstream">The master bitstream frame vars will be read from.</param>
		/// <param name="outstream">Only valid if this is flagged asServer. This is the modified bitstream forwarded to other clients after the server receives an update.</param>
		/// <param name="updateType">Enum that indicates if this update is a special case, such as a Teleport, rewind cast, or has an attached user custom message.</param>
		/// <param name="length">Specifies where this NST update ends in the master bitstream. Don't bother trying to figure out what it does.</param>
		/// <param name="asServer">Indicates that this update is being read as the server. This is needed for the server is a host, and this needs to run as both server and as client.</param>
		/// <returns></returns>
		public UpdateType ReceieveGeneric(ref UdpBitStream bitstream, ref UdpBitStream outstream, UpdateType updateType, int length, bool asServer)
		{
			// Log the bitstream pointer position
			int startPtr = bitstream.Ptr;
			// Read/Write FrameId
			int frameid = bitstream.ReadInt(NSTSettings.single.bitsForPacketCount);
			if (asServer)
				outstream.WriteInt(frameid, NSTSettings.single.bitsForPacketCount);

			BandwidthUsage.AddUsage(ref bitstream, "Frame Id");
			
			Frame frame = buffer.frames[frameid];
			mostRecentRcvdFrame = frame; // log most recently arrived frame - used by server authority

			frame.state = (State)bitstream.ReadInt(2);
			// if this is the authority owner, accept incoming state
			if (!MasterNetAdapter.ServerIsActive && na.HasAuthority)
				State = frame.state;
				//QueueLaggedAction(SetStateAlive);

			BandwidthUsage.AddUsage(ref bitstream, "State");

			frame.updateType = updateType;

			frame.rootSendType = (RootSendType)bitstream.ReadInt(2);
			BandwidthUsage.AddUsage(ref bitstream, "Root Pos Comp Type");

			// Store store the values that the owner originally sent before we start altering the frame
			CompressedElement ownrSentCompPos = frame.compPos;
			Vector3 ownrSentPos = frame.rootPos;

			// READ ROOT POS to the temp var rootCompPos
			CompressedElement rootCompPos =
				(frame.rootSendType.IsLBits()) ? WorldVectorCompression.ReadCompressedPosFromBitstream(ref bitstream, includedAxes, true) :
				(frame.rootSendType.IsFull()) ? WorldVectorCompression.ReadCompressedPosFromBitstream(ref bitstream, includedAxes, false) :
				 CompressedElement.zero;

			BandwidthUsage.AddUsage(ref bitstream, "Root Pos");

			// READ CHILD ELEMENTS (including root rotation) and animator parameters.
			foreach (INstBitstreamInjectFirst cb in iBitstreamInjects)
				cb.NSTBitstreamIncomingFirst(frame, CurrentFrame, ref bitstream, asServer, svrWaitingForOwnerTeleportConfirm);

			BandwidthUsage.AddUsage(ref bitstream, "Elements");

			// apply pos to frame
			frame.compPos = rootCompPos;
			frame.CompletePosition(lastRootCompPos);

			if (updateType.IsTeleport())
			{
				DebugX.Log(!DebugX.logInfo ? "" : 
					(Time.time + "<b> fid: " + frame.frameid + " updatetye: " + (int)updateType + " Inc Teleport </b>" + rootCompPos + " rot: " + frame.RootRot + "  type:" + frame.rootSendType + " svrwaiting?: " + svrWaitingForOwnerTeleportConfirm));

				if (na.HasAuthority)
				{
					OwnerTeleportCommandHandler(frame);
					// Set this AFTER OwnerTeleportCommandHandler, is used to check for repeated sends from server
					ownrNeedsToSendTeleportConfirm = !MasterNetAdapter.ServerIsActive; // !isServer; // Server owned objects cannot flag this as true, or they create an endless loop on server.
				}

				// Apply immediate teleports
				// TODO: Should all teleports be treated as immediate?
				if (frameid == 0 && !asServer)
				{
					ApplyTeleportLocally(frame, true);

					foreach (INstTeleportIncoming cb in iNstTeleportIncoming)
						cb.OnRcvSvrTeleportCmd(frame);
				}

				if (asServer)
					svrWaitingForOwnerTeleportConfirm = false;
			}
			else
				ownrNeedsToSendTeleportConfirm = false;

			// If this is the server and we are waiting for a confirm... don't accept new updates for teleport overriden elements/root.
			if (svrWaitingForOwnerTeleportConfirm)
			{
				frame.rootSendType = RootSendType.Full;

				frame.compPos = lastSentCompPos;
				frame.rootPos = lastSentPos;

				//  Keep resending teleport values for elements until confirmed - Move this to interface callback
				for (int eid = 0; eid < nstElementsEngine.transformElements.Length; eid++)
				{
					TransformElement te = nstElementsEngine.transformElements[eid];
					if (te.teleportOverride)
					{
						frame.elements[eid].compTrans = te.lastSentCompressed;
						frame.elements[eid].transform = te.lastSentTransform;
					}
				}
			}

			// used to let arrivals know that they shouldn't overwrite elements with TeleportOverride=true - as they may have just been overridden with a teleport value
			// and should not be changed back by outdated incoming updates.
			bool isMidTeleport = ownrNeedsToSendTeleportConfirm || !buffer.prevAppliedFrame.updateType.IsTeleport();

			// If this frame has already been extrapolated and is mid-interpolation, overwrite the lowerbits with the correct incoming ones
			// Don't overwrite if we appear to be mid teleport.
			if (!na.HasAuthority && frameid == CurrentIndex && isMidTeleport) 
			{
				//TODO may need to fire a OnStartInterpolation callback somehwere how since this means any custom data and such that arrived late never will have fired.
				frame.CompletePosition(frame.compPos);
				frame.rootPos = frame.compPos.DecompressFromWorld();
			}

			// Callback used by ServerAuthority component to get the position error value
			if (na.HasAuthority)
				foreach (INstOnOwnerIncomingRootPos cb in iNstOnOwnerIncomingRoot)
					cb.OnOwnerIncomingRootPos(frame, ownrSentCompPos, ownrSentPos);

			
			// adjust teleport flag if server has changed it (server owned objects leave alone) teleport flag to indicate of server is still waiting for a confirm.
			if (asServer && !na.HasAuthority)
			{
				if (svrWaitingForOwnerTeleportConfirm)
					frame.updateType |= UpdateType.Teleport;
				else
					frame.updateType &= ~UpdateType.Teleport;
			}

			// Server actually dictates state, so the server state is written here, rather than mirroring what the owner claimed.
			if(asServer)
				outstream.WriteInt((int)State, 2);

			// Apply ServerAuthority offset - Adjust outgoing rootPos to server delta. This delta is a bit outdated, but better than nothing.
			if (asServer)// && frame.rootSendType.IsPosType())
			{
				Vector3 errorOffset = new Vector3(0, 0, 0);
				bool outOfSync = false;

				// Collect any server side interventions to the root pos. Used by ServerAuthority component to return the offset value that should be applied
				// to the out of sync owner update.
				foreach (INstOnSvrOutgoingRootPos cb in iNstOnSvrOutgoingRoot)
				{
					bool _outofsync;
					errorOffset += cb.OnSvrOutgoingRootPos(out _outofsync);
					outOfSync |= _outofsync;
				}

				if (outOfSync)
				{
					outstream.WriteInt((int)RootSendType.Full, 2);

					frame.rootPos = frame.rootPos + errorOffset;

					frame.compPos = (frame.rootSendType.IsLBits()) ?
						frame.rootPos.CompressToWorld().GuessUpperBitsWorld(buffer.prevAppliedFrame.compPos) :
						frame.rootPos.CompressToWorld();

					frame.compPos.WriteWorldCompPosToBitstream(ref outstream, includedAxes, false);

					//TODO: Add an outgoing for none root pos updates if there is a sync error
				}

				// Write the outgoing normally without server intervention if we are not out of sync.
				else
				{
					outstream.WriteInt((int)frame.rootSendType, 2);
					if (frame.rootSendType.IsPosType())
						frame.compPos.WriteWorldCompPosToBitstream(ref outstream, includedAxes, frame.rootSendType.IsLBits());
				}
			}

			DebugX.Log(!DebugX.logInfo ? "" : (Time.time + " NST:" + NstId + " " + name + " " + updateType + " " + frame.rootSendType + " <color=green><b>RCV Update " +
				frameid + " </b></color>\n" + frame.compPos + "  " + frame.rootPos + " <color=purple>" +  frame.RootRot.type + " " + ((Quaternion)frame.RootRot).eulerAngles + "</color> " + 
				updateType + " size:" + bitstream.Length + " bits"));

			// Callback used by server to forward child elements and animator params to clients. 
			if (asServer)
				foreach (INstBitstreamInjectFirst cb in iBitstreamInjects)
					cb.NSTBitstreamMirrorFirst(frame, ref outstream, asServer, svrWaitingForOwnerTeleportConfirm);
			
			// READ animator parameters.
			// TODO: outstream write needs to be decoupled
			foreach (INstBitstreamInjectSecond cb in iBitstreamInjectSecond)
				cb.NSTBitstreamIncomingSecond(frame, CurrentFrame, ref bitstream, ref outstream, asServer, svrWaitingForOwnerTeleportConfirm);
			BandwidthUsage.AddUsage(ref bitstream, "Animator");

			// Callback used by Rewind to read out any casts and apply to the rewind ghost for this object
			foreach (INstBitstreamInjectThird cb in iBitstreamInjectsLate)
				cb.NSTBitstreamIncomingThird(frame, ref bitstream, ref outstream, asServer);
			BandwidthUsage.AddUsage(ref bitstream, "Casts");

			DebugWidget.Move(debugXformGO, frame.rootPos, frame.RootRot, debugXform, DebugXform.RawReceive);

			// if this is a Custom message, pass it along as an event.
			if (updateType.IsCustom())
			{
				// measure what is left in this update buffer - sent that out as the custom message
				int remainingBits = (length << 3) - (bitstream.Ptr - startPtr);
				int remainingBytes = (remainingBits >> 3) + ((remainingBits % 8 == 0) ? 0 : 1);

				// Read the custom data directly into the frame buffer to skip some steps. 
				bitstream.ReadByteArray(frame.customData, remainingBytes);

				if (asServer)
					outstream.WriteByteArray(frame.customData, remainingBytes);

				frame.customMsgSize = remainingBytes;
			}

			BandwidthUsage.AddUsage(ref bitstream, "Custom");
			BandwidthUsage.PrintSummary();

			foreach (INstOnRcvUpdate cb in iNstOnRcvUpdate)
				cb.OnRcv(frame);

			//This belongs AFTER all alterations to incoming pos/rot have been made
			// Store the final adjusted rootCompPos for next update to use for upperbits guessing.
			lastRootCompPos = frame.compPos;

			// If this frame came late and is now mid-interpolation... notify interface callbacks (authority items will always be current - ignore them)
			if (frameid == CurrentIndex && !na.HasAuthority)
			{
				foreach (INstOnStartInterpolate cb in iNstOnStartInterpolate)
					cb.OnStartInterpolate(frame, true, isMidTeleport);
			}
			// Send all offtick events now, since they will never interpolate or be added to the buffer, and return since this won't be added to buffer.
			// TODO not so sure about firing these callbacks
			else if (frameid == 0)
			{
				foreach(INstOnStartInterpolate cb in iNstOnStartInterpolate)
					cb.OnStartInterpolate(frame);

				foreach(INstOnEndInterpolate cb in iNstOnEndInterpolate)
					cb.OnEndInterpolate(frame);

				return frame.updateType;
			}

			// if update is required for this pass (client or headless server) Update pos - add to lastpos in the case of pos_delta
			if (!na.HasAuthority && frameid != 0)
			{
				buffer.AddFrameToBuffer(frame);
			}

			return frame.updateType;
		}

		#endregion

	}
}
