//Copyright 2018, Davin Carten, All rights reserved

using UnityEngine;
using UnityEditor;

namespace emotitron.Network.NST
{
	[CustomEditor(typeof(NetworkSyncTransform))]
	[CanEditMultipleObjects]
	public class NSTEditor : NSTHeaderEditorBase
	{
		NetworkSyncTransform nst;

		public override void OnEnable()
		{
			AddAllDependencies(false);
		}

		private void AddAllDependencies(bool silence = false)
		{
			headerName = HeaderNSTName;
			headerColor = HeaderNSTColor;
			base.OnEnable();

			nst = (NetworkSyncTransform)target;

			// Add this NST to the prefab spawn list (and as player prefab if none exists yet) as an idiot prevention
			if (!Application.isPlaying)
				MasterNetAdapter.AddAsRegisteredPrefab(nst.gameObject, silence);


			// If user tried to put NST where it shouldn't be... remove it and all of the required components it added.
			if (nst.transform.parent != null)
			{
				Debug.LogError("NetworkSyncTransform must be on the root of an prefab object.");
				nst.nstElementsEngine = nst.transform.GetComponent<NSTElementsEngine>();

				NSTNetAdapter.RemoveAdapter(nst);

				DestroyImmediate(nst);

				if (nst.nstElementsEngine != null)
					DestroyImmediate(nst.nstElementsEngine);

				return;
			}

			nst.na = NSTNetAdapter.EnsureNstAdapterExists(nst.gameObject);

			nst.nstSettings = NSTSettings.EnsureExistsInScene(NSTSettings.DEFAULT_GO_NAME);
			nst.nstElementsEngine = NSTElementsEngine.EnsureExistsOnRoot(nst.transform, false);
			NSTMaster.EnsureExistsInScene("NST Master");


		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			AddAllDependencies(true);

			NetworkSyncTransform nst = (NetworkSyncTransform)target;
			
			// Make sure the object is active, to prevent users from spawning inactive gameobjects (which will break things)
			if (!nst.gameObject.activeSelf)// && AssetDatabase.Contains(target))
			{
				Debug.LogWarning("Prefabs with NetworkSyncTransform on them MUST be enabled. If you are trying to disable this so it isn't in your scene when you test it, no worries - NST destroys all scene objects with the NST component at startup.");
				nst.gameObject.SetActive(true);
			}

			NSTSettings NSTSettings = NSTSettings.EnsureExistsInScene(NSTSettings.DEFAULT_GO_NAME);
			Rect r = EditorGUILayout.GetControlRect();
			GUI.Label(r, "Summary", "BoldLabel");
			EditorGUILayout.HelpBox(
				//"Summary:\n" +
				"Approx max seconds of buffer " + ((1 << NSTSettings.bitsForPacketCount) - 2) * Time.fixedDeltaTime * nst.sendEveryXTick * NSTSettings.TickEveryXFixed * 0.5f + " \n" +
				"sendEveryXTick = " + nst.sendEveryXTick + "\n" +
				"NSTSettings.bitsForPacketCount = " + NSTSettings.bitsForPacketCount + "\n" +
				"Time.fixedDeltaTime = " + Time.fixedDeltaTime
				,
				MessageType.None);

		}

		private float MaxSecondsOfBuffer()
		{
			return ((1 << NSTSettings.EnsureExistsInScene(NSTSettings.DEFAULT_GO_NAME).bitsForPacketCount) - 2) * Time.fixedDeltaTime * nst.sendEveryXTick * NSTSettings.single.TickEveryXFixed * 0.5f;
		}
	}
}