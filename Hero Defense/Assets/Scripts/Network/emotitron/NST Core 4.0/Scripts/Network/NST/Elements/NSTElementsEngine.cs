//Copyright 2018, Davin Carten, All rights reserved

using System.Collections.Generic;
using UnityEngine;
using emotitron.Utilities.SmartVars;
using emotitron.Network.Compression;
using emotitron.Network.NST.Rewind;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace emotitron.Network.NST
{
	/// <summary>
	/// Root NST Component that collects all NSTRotation and NSTPosition components on fires them accordingly to the callback interfaces of the NST.
	/// </summary>
	public class NSTElementsEngine : NSTRootSingleton<NSTElementsEngine>, INstBitstreamInjectFirst, INstOnExtrapolate, INstOnReconstructMissing, INstOnStartInterpolate, INstOnInterpolate, //, INstOnSndUpdate, INstOnRcvUpdate, INstOnEndInterpolate
			 INstOnSndUpdate, IRewind, IRewindGhostsToFrame, INstOnSnapshotToRewind, ICreateRewindGhost, INstTeleportIncoming
	{
		[HideInInspector] public TransformElement[] transformElements;
		[HideInInspector] public GenericX[][] history; // List[frameid][elementid]
		[HideInInspector] public int rootRotId;

		// TODO hash these strings or String.Intern or something ?
		public Dictionary<string, int> elementIdLookup;
		public Dictionary<string, TransformElement> elementLookup;

		public GenericX GetRewind(int elementId, int frameid)
		{
			return history[frameid][elementId];
		}

		public override void OnNstPostAwake()
		{
			base.OnNstPostAwake();
			Initialize();
		}

		private bool initialized;
		public NSTElementsEngine Initialize()
		{
			if (initialized)
				return this;

			initialized = true;

			// Collect all of the transform elements
			INSTTransformElement[] iTransElement = GetComponentsInChildren<INSTTransformElement>(true);
			elementIdLookup = new Dictionary<string, int>(iTransElement.Length);
			elementLookup = new Dictionary<string, TransformElement>(iTransElement.Length);

			transformElements = new TransformElement[iTransElement.Length];
			for (int i = 0; i < iTransElement.Length; i++)
			{
				if (elementIdLookup.ContainsKey(iTransElement[i].TransElement.name))
				{
					DebugX.LogError(!DebugX.logErrors ? "" : 
						("Multiple child elements with the same name on '" + nst.name + "'. Check the names of Rotation and Positon elements for any repeats and be sure they all have unique names."));
				}
				else
				{
					elementIdLookup.Add(iTransElement[i].TransElement.name, i);
					elementLookup.Add(iTransElement[i].TransElement.name, iTransElement[i].TransElement);
				}

				// Make note of which of the transforms belongs to the NST root rotation
				if (transformElements[i] == nst.rootRotationElement)
				{
					rootRotId = i;
					transformElements[0] = transformElements[i];
				}

				transformElements[i] = iTransElement[i].TransElement;
				transformElements[i].index = i;

				if (transformElements[i].gameobject == null)
					transformElements[i].gameobject = iTransElement[i].GameObject;

				transformElements[i].Initialize(nst);
			}

			// init the list
			history = new GenericX[NSTSettings.single.frameCount][];
			for (int frameid = 0; frameid < history.Length; frameid++)
			{
				history[frameid] = new GenericX[transformElements.Length];
				for (int elementid = 0; elementid < transformElements.Length; elementid++)
				{
					history[frameid][elementid] = new GenericX();
				}
			}
			return this;
		}

		public void NSTBitstreamOutgoingFirst(Frame frame, ref UdpBitStream bitstream)
		{
			for (int elementid = 0; elementid < transformElements.Length; elementid++)
			{
				transformElements[elementid].Write(ref bitstream, frame);

				// Write to the local buffer
				frame.elements[elementid].transform = transformElements[elementid].Localized;
			}
		}

		// callback from NST, extract transform elements
		public void NSTBitstreamIncomingFirst(Frame frame, Frame currFrame, ref UdpBitStream bitstream, bool isServer, bool waitingForTeleportConfirm)
		{
			for (int eid = 0; eid < transformElements.Length; eid++)
			{
				frame.elements[eid].hasChanged = transformElements[eid].Read(ref bitstream, frame, currFrame);
			}
		}

		public void NSTBitstreamMirrorFirst(Frame frame, ref UdpBitStream outstream, bool isServer, bool waitingForTeleportConfirm)
		{
			for (int eid = 0; eid < transformElements.Length; eid++)
				transformElements[eid].MirrorToClients(ref outstream, frame, frame.elements[eid].hasChanged); // masks[eid].GetBitInMask(frame.frameid));
		}

		public void OnSvrTeleportCmd()
		{
			for (int eid = 0; eid < transformElements.Length; eid++)
			{
				TransformElement te = transformElements[eid];
				if (te.teleportOverride)
					te.Teleport();
			}
				
		}

		public void OnTeleportApply(Frame frame)
		{
			for (int eid = 0; eid < transformElements.Length; eid++)
			{
				TransformElement te = transformElements[eid];
				if (te.teleportOverride)
				{
					te.Teleport(frame.elements[eid].transform);
				}
			}
		}

		/// <summary>
		/// Teleport all elements that are flagged with teleportOverride = true;
		/// </summary>
		public void OnRcvSvrTeleportCmd(Frame frame)
		{
			for (int eid = 0; eid < transformElements.Length; eid++)
			{
				TransformElement te = transformElements[eid];

				// TODO: this likely is only wired to work correctly with offtick
				if (te.teleportOverride)
				{
					te.Teleport(frame.elements[eid].compTrans, frame.elements[eid].transform);
				}
			}
		}

		public void OnStartInterpolate(Frame frame, bool lateArrival = false, bool midTeleport = false)
		{
			// Don't apply the transform for frame 0 updates. Those are for teleports and weapon fire.
			if (frame.frameid == 0)// && !frame.updateType.IsTeleport())
				return;

			for (int eid = 0; eid < transformElements.Length; eid++)
			{
				TransformElement te = transformElements[eid];
				// Don't overwrite mid interpolation if this is a teleport override element, and we are mid teleport.
				if (lateArrival && midTeleport && te.teleportOverride)
					continue;

				// Don't modify elements with late arriving data if it is null.
				if (lateArrival && frame.elements[eid].transform.type == XType.NULL)
				{
					DebugX.Log(!DebugX.logInfo ? "" :
						(Time.time + " <b>Null Late Arrival - NOTE if you keep seeing this davin - remove this test otherwise </b> " + te.snapshot + " " + te.target));

					continue;
				}

				te.Snapshot(frame, lateArrival, midTeleport);
			}
		}

		public void OnInterpolate(float t)
		{
			for (int eid = 0; eid < transformElements.Length; eid++)
			{
				transformElements[eid].UpdateInterpolation(t);
			}
		}
		
		/// <summary>
		/// Extrapolate is used when the buffer is empty.
		/// </summary>
		public void OnExtrapolate(Frame targFr, Frame currFr, int extrapolationCount, bool svrWaitingForTeleportConfirm)
		{
			for (int eid = 0; eid < transformElements.Length; eid++)
			{
				TransformElement te = transformElements[eid];

				bool currIsNull = currFr.elements[eid].transform.type == XType.NULL;

				// repeat teleportoverride if this is server and is waiting for a teleport confirm
				bool svrWaiting = svrWaitingForTeleportConfirm && te.teleportOverride; // || currFr.updateType.IsTeleport() && te.teleportOverride;

				// Don't extrapolate if this was a teleport or if we are exceeded the max number of sequential extrapolates
				bool dontExtrapolate = (currFr.updateType.IsTeleport() || extrapolationCount >= te.maxExtrapolates);

				targFr.elements[eid].transform =
					svrWaiting ? te.lastSentTransform :
					dontExtrapolate ? currIsNull ? te.Localized : currFr.elements[eid].transform : // If the current frame we are extrapolating was a teleport... just copy
					te.Extrapolate();

				targFr.elements[eid].compTrans = 
					svrWaiting ? te.lastSentCompressed :
					dontExtrapolate ? currFr.elements[eid].compTrans : // If the current frame we are extrapolating was a teleport... just copy
					te.Compress(targFr.elements[eid].transform);

			}
		}

		/// <summary>
		/// Unlike Extrapolate - Reconstruct is used when the buffer isn't empty, but rather we are dealing with a lost packet while there is a future frame in the buffer.
		/// </summary>
		public void OnReconstructMissing(Frame nextFrame, Frame currentFrame, Frame nextValidFrame, float t, bool svrWaitingForTeleportConfirm)
		{

			// Reconstruct missing frames
			for (int eid = 0; eid < transformElements.Length; eid++)
			{
				TransformElement te = transformElements[eid];

				//TODO are these if's needed for the null checking? Keep an eye on this.
				// Eliminate any Null genericX values = they indicate no changes
				if (currentFrame.elements[eid].transform.type == XType.NULL)
				{
					currentFrame.elements[eid].transform = te.Localized;
					currentFrame.elements[eid].compTrans = te.Compress(currentFrame.elements[eid].transform);

					Debug.Log("Current element is null");
				}

				if (nextValidFrame.elements[eid].transform.type == XType.NULL)
				{
					nextValidFrame.elements[eid].transform = currentFrame.elements[eid].transform;
					//Debug.LogError("nextvalid element is null");
				}


				// If server his holding for teleport confirm, keep using the same teleport value
				if (svrWaitingForTeleportConfirm && te.teleportOverride)
				{
					nextFrame.elements[eid].transform = te.lastSentTransform;
					nextFrame.elements[eid].compTrans = te.lastSentCompressed;
				}
				// There is a future frame to use as a guess target
				else if (nextValidFrame.elements[eid].transform.type != XType.NULL) // nst.buffer.masks[eid].GetBitInMask(nextValidFrame.frameid))
				{
					nextFrame.elements[eid].transform = te.Lerp(currentFrame.elements[eid].transform, nextValidFrame.elements[eid].transform, t);
					nextFrame.elements[eid].compTrans = te.Compress(nextFrame.elements[eid].transform);
				}
				// There is no future frame.
				else
				{
					if (currentFrame.elements[eid].transform.type == XType.NULL)
						Debug.Log("Houston we have a null here.");

					nextFrame.elements[eid].transform = currentFrame.elements[eid].transform;
					nextFrame.elements[eid].compTrans = currentFrame.elements[eid].compTrans;
				}
			}
		}

		public void OnRewindGhostsToFrame(Frame frame)
		{
			for (int i = 0; i < transformElements.Length; i++)
				transformElements[i].Apply(frame.elements[i].transform, transformElements[i].rewindGO);
		}

		/// <summary>
		/// If a Rewind request has been made, this callback interface is called on all registered elements. Each element will populate its history[0] frame with the resuts of the requested rewind time.
		/// If applyToGhost is true, it will also apply its rewound result to its element on the rewindGhost for this NST.
		/// </summary>
		public void OnRewind(HistoryFrame fe, GameObject rewindGo, int startFrameid, int endFrameId, float timeBeforeSnapshot, float remainder, bool applyToGhost)
		{
			for (int i = 0; i < history[0].Length; i++)
			{
				//TODO: this needs to slerp for rotation types
				history[0][i] = (timeBeforeSnapshot > 0) ?
					Vector3.Lerp(history[startFrameid][i], history[endFrameId][i], remainder) :
					Vector3.Lerp(history[startFrameid][i], transformElements[i].Localized, -remainder);

				if (applyToGhost)
				{
					transformElements[i].Apply(history[0][i], transformElements[i].rewindGO);
				}
			}
		}

		// Snapshot local auth objects on send, since they don't interpolate. 
		public void OnSnd(Frame frame)
		{
			if (MasterNetAdapter.ServerIsActive && frame.frameid != 0)
			{
				OnSnapshotToRewind(frame);
			}
		}

		public void OnSnapshotToRewind(Frame frame)
		{
			for (int i = 0; i < transformElements.Length; i++)
			{
				history[frame.frameid][i] = transformElements[i].Localized;
			}
		}

		public void OnCreateGhost(GameObject srcGO, GameObject ghostGO)
		{
			for (int i = 0; i < transformElements.Length; i++)
				if (srcGO == transformElements[i].gameobject)
					transformElements[i].rewindGO = ghostGO;
		}
	}


#if UNITY_EDITOR

	[CustomEditor(typeof(NSTElementsEngine))]
	[CanEditMultipleObjects]
	public class NSTElementsEngineEditor : NSTHeaderEditorBase
	{
		public override void OnEnable()
		{
			headerName = HeaderElementsEngineName;
			headerColor = HeaderEngineColor;
			base.OnEnable();
		}
	}
#endif
}

