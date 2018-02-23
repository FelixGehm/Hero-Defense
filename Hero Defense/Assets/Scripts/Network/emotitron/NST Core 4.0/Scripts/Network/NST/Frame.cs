//Copyright 2018, Davin Carten, All rights reserved

using System.Collections.Generic;
using UnityEngine;
using emotitron.Utilities.SmartVars;
using UnityEngine.Events;

namespace emotitron.Network.NST

{
	public class HistoryFrame
	{
		public int frameid;
		public float endTime;
		public Vector3 rootPosition;

		public HistoryFrame(int _frameid, Vector3 _pos, Quaternion _rot)
		{
			frameid = _frameid;
			rootPosition = _pos;
		}
	}

	public class Frame : UnityEvent<Frame>
	{
		public NetworkSyncTransform nst;
		public NSTElementsEngine nstElementsEngine;
		public int frameid;
		public float packetArriveTime;
		public float appliedTime;
		public float endTime;
		public UpdateType updateType;
		public RootSendType rootSendType;
		public CompressedElement compPos;
		public State state;

		public class XElement
		{
			public GenericX transform;
			public CompressedElement compTrans;
			public bool hasChanged;
			public TransformElement transformElement;

			public XElement(GenericX transform, CompressedElement compTrans, bool hasChanged, TransformElement transformElement)
			{
				this.transform = transform;
				this.compTrans = compTrans;
				this.hasChanged = hasChanged;
				this.transformElement = transformElement;
			}
		}

		public List<XElement> elements;

		public Vector3 rootPos;
		public byte[] customData;
		public int customMsgSize;
		public int customMsgPtr;

		/// <summary>
		/// Sets both the root position and compressed root position values with one call.
		/// </summary>
		public Vector3 RootPos
		{
			get { return rootPos; }
			set
			{
				rootPos = value;
				compPos = value.CompressToWorld();
			}
		}

		/// <summary>
		/// Sets both the root position and compressed root position values with one call.
		/// </summary>
		public CompressedElement CompRootPos
		{
			get { return compPos;  }
			set
			{
				compPos = value;
				rootPos = value.DecompressFromWorld();
			}
		}

		/// <summary>
		/// Accesses the Root Rotation from the elements engine
		/// </summary>
		public GenericX RootRot
		{
			get { return elements[nstElementsEngine.rootRotId].transform; }
			set { elements[nstElementsEngine.rootRotId].transform = value; }
		}

		public CompressedElement CompRootRot
		{
			get { return elements[nstElementsEngine.rootRotId].compTrans; }
			set { elements[nstElementsEngine.rootRotId].compTrans = value; }
		}

		// Construct
		public Frame(NetworkSyncTransform _nst, NSTElementsEngine _nstElementsEngine, int index, Vector3 _pos, CompressedElement _compPos, Quaternion _rot) //, PositionElement[] positionElements, RotationElement[] rotationElements)
		{
			nst = _nst;
			nstElementsEngine = _nstElementsEngine;
			rootPos = _pos;
			compPos = _compPos;

			int numbOfElements = nstElementsEngine.transformElements.Length;

			elements = new List<XElement>(numbOfElements);
			for (int eid = 0; eid < numbOfElements; eid++)
			{
				elements.Add(new XElement(
					nstElementsEngine.transformElements[eid].Localized,
					nstElementsEngine.transformElements[eid].Compress(), 
					false, 
					nstElementsEngine.transformElements[eid]
					));
			}

			frameid = index;
			customData = new byte[128];  //TODO: Make this size a user setting
		}

		public void ModifyFrame(UpdateType _updateType, RootSendType _rootSendType, Vector3 _pos, Quaternion _rot, float _packetArrivedTime)
		{
			updateType = _updateType;
			rootSendType = _rootSendType;

			rootPos = _pos;
			compPos = _pos.CompressToWorld();

			RootRot = _rot;

			CompRootRot = nstElementsEngine.transformElements[0].Compress(_rot);
			packetArriveTime = _packetArrivedTime;
		}

		
		/// <summary>
		/// Guess the correct upperbits using the supplied frame for its compressedPos as a starting point. Will find the upperbits that result in the least movement from that pos.
		/// If rootPos is emoty, it will entirely copy the position from the supplied previous frame.
		/// </summary>
		public void CompletePosition(Frame prevCompleteframe)
		{
			CompletePosition(prevCompleteframe.compPos);
		}

		public void CompletePosition(CompressedElement prevComplete)
		{
			if (rootSendType.IsLBits())
				compPos = compPos.GuessUpperBitsWorld(prevComplete);

			// no new position is part of this update - copy the old
			else if (!rootSendType.IsPosType())
				compPos = prevComplete;

			// now handled by the get set
			rootPos = compPos.DecompressFromWorld();

		}

		/// <summary>
		/// Apply all of the current transforms to this frames stored transforms.
		/// </summary>
		public void CaptureCurrentTransforms()
		{
			updateType = UpdateType.Teleport;
			rootSendType = RootSendType.Full;

			RootPos = nst.transform.position;

			for (int eid = 0; eid < elements.Count; eid++)
			{
				TransformElement te = nstElementsEngine.transformElements[eid];

				elements[eid].transform = te.Localized;
				elements[eid].compTrans = te.Compress();
			}
		}

		public override string ToString()
		{
			string e = " Changed Elements: ";
			for (int eid = 0; eid < elements.Count; eid++)
				if (elements[eid].hasChanged)
					e += eid + " ";

			return
				"FrameID: " + frameid + " " + updateType + "  " + rootSendType + " " + state + "  " +  e + "\n" + 
				"compPos: " + compPos + " pos: " + rootPos + "\n" +
				"compRot: " + CompRootRot + " rot: " + RootRot;
		}
	}
}
