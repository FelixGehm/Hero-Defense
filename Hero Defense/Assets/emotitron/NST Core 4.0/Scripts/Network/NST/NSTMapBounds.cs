//Copyright 2018, Davin Carten, All rights reserved

using System.Collections.Generic;
using UnityEngine;
using emotitron.Utilities;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace emotitron.Network.NST
{

	public enum FactorBoundsOn { EnableDisable, AwakeDestroy}
	/// <summary>
	/// Put this object on the root of a game map. It needs to encompass all of the areas the player is capable of moving to.
	/// The object must contain a MeshRenderer in order to get the bounds.
	/// Used by the NetworkSyncTransform to scale Vector3 position floats into integers for newtwork compression.
	/// </summary>
	//[ExecuteInEditMode]
	[HelpURL("https://docs.google.com/document/d/1nPWGC_2xa6t4f9P0sI7wAe4osrg4UP0n_9BVYnr5dkQ/edit#heading=h.4n2gizaw79m0")]
	[AddComponentMenu("Network Sync Transform/NST Map Bounds")]
	public class NSTMapBounds : MonoBehaviour
	{
		//public enum BoundsTools { Both, MeshRenderer, Collider }
		public bool includeChildren = true;

		[Tooltip("Awake/Destroy will consider a map element into the world size as long as it exists in the scene (You may need to wake it though). Enable/Disable only factors it in if it is active.")]
		[HideInInspector]
		public BoundsTools.BoundsType factorIn = BoundsTools.BoundsType.Both;
		
		// sum of all bounds (children included)
		[HideInInspector] public Bounds myBounds;
		[HideInInspector] public int myBoundsCount;

		// All bounds accounted for (in case there are more than one active Bounds objects
		private static Bounds combinedWorldBounds;
		public static Bounds CombinedWorldBounds
		{
			get
			{
				if (NSTSettings.single != null && (activeBounds == null || ActiveBoundsCount == 0))
					return NSTSettings.single.defaultWorldBounds;

				return combinedWorldBounds;
			}
		}
		
		private static List<NSTMapBounds> activeBounds = new List<NSTMapBounds>();
		public static int ActiveBoundsCount { get { return activeBounds.Count; }  }
		public static bool isInitialized;

		void Awake()
		{
			CollectMyBounds();
		}

		public void CollectMyBounds()
		{
			myBounds = BoundsTools.CollectMyBounds(gameObject, factorIn, out myBoundsCount, true);
			if (myBoundsCount > 0 && enabled)
			{
				if (!activeBounds.Contains(this))
					activeBounds.Add(this);
			}
			else
			{
				if (activeBounds.Contains(this))
					activeBounds.Remove(this);
			}
		}

		private void Start()
		{
			// only send out the initial bounds update once
			if (isInitialized)
				return;

			isInitialized = true;

			NotifyOtherClassesOfBoundsChange(!isInitialized);
		}

		private void OnEnable()
		{
			FactorInBounds(!isInitialized);
		}

		static bool isShuttingDown;

		void OnApplicationQuit()
		{
			isShuttingDown = true;
		}

		private void OnDisable()
		{
			FactorInBounds(isShuttingDown);
		}

		private void FactorInBounds(bool b, bool silent = false)
		{
			if (this == null)
				return;

			if (b)
			{
				if (!activeBounds.Contains(this))
					activeBounds.Add(this);
			}
			else
			{
				activeBounds.Remove(this);
			}

			RecalculateCombinedBounds(silent);

			// Notify affected classes of the world size change.
			if (isInitialized && Application.isPlaying)
				NotifyOtherClassesOfBoundsChange(!isInitialized); // isInitialized is to silence startup log messages
		}


		/// <summary>
		/// Whenever an instance of NSTMapBounds gets removed, the combinedWorldBounds needs to be rebuilt with this.
		/// </summary>
		public static void RecalculateCombinedBounds(bool silent = false)
		{
			// dont bother with any of this if we are just shutting down.
			if (isShuttingDown)
				return;

			if (activeBounds.Count == 0)
			{
				DebugX.LogWarning(!DebugX.logWarnings ? "" : ("There are now no active NSTMapBounds components in the scene."), !silent);
				return;
			}

			combinedWorldBounds = activeBounds[0].myBounds;
			for (int i = 1; i < activeBounds.Count; i++)
			{
				combinedWorldBounds.Encapsulate(activeBounds[i].myBounds);
			}

		}

		public static void NotifyOtherClassesOfBoundsChange(bool silent = false)
		{
			// No log messages if commanded, if just starting up, or just shutting down.
			bool isSilent = silent || !isInitialized || isShuttingDown;
			WorldVectorCompression.EstablishMinBitsPerAxis(combinedWorldBounds, isSilent);
		}
	}

#if UNITY_EDITOR

	[CustomEditor(typeof(NSTMapBounds))]
	[CanEditMultipleObjects]
	public class NSTMapBoundsEditor : Network.NST.NSTHelperEditorBase
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			var _target = (NSTMapBounds)target;

			_target.factorIn = (BoundsTools.BoundsType)EditorGUILayout.EnumPopup("Factor In", _target.factorIn);

			EditorGUILayout.HelpBox(
				"Contains " + _target.myBoundsCount + " bound(s) objects:\n" +
				"Center: " + _target.myBounds.center + "\n" +
				"Size: " + _target.myBounds.size + "\n\n" +
				
				WorldBoundsSummary(), MessageType.None);
		}

		/// <summary>
		/// Completely refinds and inventories ALL NstMapBounds in the scene, rather than dicking around trying to be efficient. This is editor only so just brute force will do.
		/// </summary>
		/// <returns></returns>
		public static string WorldBoundsSummary()
		{
			// Find every damn NSTMapBounds in the scene currently and get its bounds.
			NSTMapBounds[] all = Resources.FindObjectsOfTypeAll<NSTMapBounds>();
			foreach (NSTMapBounds mb in all)
			{
				PrefabType type = PrefabUtility.GetPrefabType(mb);
				if (type != PrefabType.Prefab || type != PrefabType.ModelPrefab)
					mb.CollectMyBounds();
			}

			NSTMapBounds.RecalculateCombinedBounds(true);
			NSTMapBounds.NotifyOtherClassesOfBoundsChange(true);

			return
				"World Bounds \n" +
				((NSTMapBounds.ActiveBoundsCount == 0) ? 
					("No Active NSTMapBounds - using default.\n") :
					("(All " + NSTMapBounds.ActiveBoundsCount + " NSTMapBound(s) combined):\n")
					) +
					
				"Center: " + NSTMapBounds.CombinedWorldBounds.center + "\n" +
				"Size: " + NSTMapBounds.CombinedWorldBounds.size + "\n\n" +

				"Root position keyframes will use:\n" +
				"x:" + WorldVectorCompression.axisRanges[0].bits + " bits, y:" + WorldVectorCompression.axisRanges[1].bits + "bits, and z:" + WorldVectorCompression.axisRanges[2].bits + "\n\n" +

				"Root position lowerbit delta frames will use:\n" +
				"x:" + WorldVectorCompression.axisRanges[0].lowerBits + " bits, y:" + WorldVectorCompression.axisRanges[1].lowerBits + "bits, and z:" + WorldVectorCompression.axisRanges[2].lowerBits
				;
		}
	}

#endif
}

