
using UnityEngine;
using UnityEditor;

namespace emotitron.Network.NST
{
	[CustomEditor(typeof(NSTSettings))]
	[CanEditMultipleObjects]
	public class NSTSettingsEditor : NSTHeaderEditorBase
	{
		public void Awake()
		{
			NSTSettings.single = (NSTSettings)target;
		}

		public override void OnEnable()
		{
			headerName = HeaderSettingsName;
			headerColor = HeaderSettingsColor;
			base.OnEnable();

			if (NSTSettings.single != null && (NSTSettings)target != NSTSettings.single)
			{
				DestroyImmediate(target);
				Debug.LogWarning("Enforcing NSTSettings singleton. Deleting newly created NSTSettings. \n" +
					"Existing NSTSettings is in scene object <b>" + NSTSettings.single.name + "</b>");
			}
			else
				NSTSettings.single = (NSTSettings)target;

			DebugX.logWarnings = NSTSettings.single.logWarnings;
			DebugX.logInfo = NSTSettings.single.logTestingInfo;
		}

		public override void OnInspectorGUI()
		{

			base.OnInspectorGUI();

			serializedObject.Update();

			NSTMaster.EnsureExistsInScene("NST Master").EnsureHasCorrectAdapter();

			var nstsettings = (NSTSettings)target;

			nstsettings.MaxNSTObjects = (uint)System.Math.Pow(2, nstsettings.bitsForNstId);
			nstsettings.frameCount = (int)System.Math.Pow(2, nstsettings.bitsForPacketCount);

			float adjustedFixedTime = Time.fixedDeltaTime;

			EditorGUILayout.Space();
			EditorGUILayout.LabelField(new GUIContent("Summary:"), "BoldLabel");

			string str =
				"Physics Rate: " + adjustedFixedTime.ToString("0.000") + "ms (" + (1 / adjustedFixedTime).ToString("0.0") + " ticks/sec)\n\n" +

				"You can change the physics rate by changing the Edit/Project Settings/Time/Fixed Step value. \n\n" +

				NSTMapBoundsEditor.WorldBoundsSummary()
				;

			EditorGUILayout.HelpBox(str, MessageType.None);

			serializedObject.ApplyModifiedProperties();


		}


		private void OnDestroy()
		{
			if (NSTSettings.single == target)
				NSTSettings.single = null;
		}

		public void ChangeNetworkingEngine(NetworkLibrary netLib)
		{
			Debug.Log("Change to " + netLib);
		}


		[MenuItem("GameObject/Network Sync Transform/Add Settings and Master", false, 10)]
		static void CreateCustomGameObject(MenuCommand menuCommand)
		{
			AddNSTSettingsGOFromMenu();
		}

		// Add NSTSettings to scene with default settings if it doesn't already contain one.
		private static void AddNSTSettingsGOFromMenu()
		{
			NSTSettings.EnsureExistsInScene(NSTSettings.DEFAULT_GO_NAME);
		}

		private static void AddMapBounds()
		{
			MeshRenderer[] renderers = Selection.activeGameObject.GetComponents<MeshRenderer>();
			if (renderers.Length == 0)
			{
				Debug.LogWarning("NSTMapBounds added to an item that has no Mesh Renderers in its tree.");
			}
			Selection.activeGameObject.AddComponent<NSTMapBounds>();
		}
	}
}

