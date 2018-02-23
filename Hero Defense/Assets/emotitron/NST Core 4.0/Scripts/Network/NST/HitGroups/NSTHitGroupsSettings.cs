//Copyright 2018, Davin Carten, All rights reserved

using System.Collections.Generic;
using emotitron.Utilities.BitUtilities;
using emotitron.Utilities.GUIUtilities;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace emotitron.Network.NST
{
	public class NSTHitGroupsSettings : Singleton<NSTHitGroupsSettings>
	{
		public static bool initialized;
		public const string DEF_NAME = "Default";

		protected override void Awake()
		{
			base.Awake();

			Initialize();
		}

		[SerializeField]
		[HideInInspector]
		public List<string> hitGroupTags = new List<string>(1) { DEF_NAME };

		public Dictionary<string, int> rewindLayerTagToId = new Dictionary<string, int>();

		public void Initialize()
		{
			if (initialized)
				return;

			initialized = true;

			for (int i = 0; i < hitGroupTags.Count; i++)
				if (rewindLayerTagToId.ContainsKey(hitGroupTags[i]))
				{
					DebugX.LogError(!DebugX.logErrors ? "" : ("The tag '" + hitGroupTags[i] + "' is used more than once in NSTRewindSettings. Repeats will be discarded, which will likely break some parts of rewind until they are removed."));
				}
				else
				{
					rewindLayerTagToId.Add(hitGroupTags[i], i);
				}

			DebugX.Log(!DebugX.logInfo ? "" : ("Initialized NSTHitboxSettings - Total Layer Tags Count: " + hitGroupTags.Count));
		}

		/// <summary>
		/// Supplied a previous index and hitgroup name, and will return the index of the best guess in the current list of layer tags. First checks for name,
		/// then if the previous int still exists, if none of the above returns 0;
		/// </summary>
		/// <returns></returns>
		public static int FindClosestMatch(string n, int id)
		{
			var hgs = EnsureExistsInScene(NSTSettings.DEFAULT_GO_NAME);
			if (hgs.hitGroupTags.Contains(n))
				return hgs.hitGroupTags.IndexOf(n);
			if (id < hgs.hitGroupTags.Count)
				return id;

			return 0;
		}
	}

#if UNITY_EDITOR

	[CustomEditor(typeof(NSTHitGroupsSettings))]
	[CanEditMultipleObjects]
	public class NSTHitGroupsSettingsEditor : NSTHeaderEditorBase
	{
		GUIStyle bold;
		NSTHitGroupsSettings _target;
		
		public override void OnEnable()
		{
			headerName = HeaderSettingsName;
			headerColor = HeaderSettingsColor;
			base.OnEnable();

		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			_target = NSTHitGroupsSettings.EnsureExistsInScene(NSTSettings.DEFAULT_GO_NAME);

			EditorGUILayout.Space();

			EditorGUILayout.BeginVertical("flow overlay box");
			EditorGUILayout.Space();

			Rect rt = EditorGUILayout.GetControlRect();

			float padding = 5f;
			float xButtonWidth = 16f;
			float fieldLeft = rt.xMin + EditorGUIUtility.labelWidth - padding;// rt.width - EditorGUIUtility.fieldWidth;
			float fieldWidth = rt.width - fieldLeft - padding; // EditorGUIUtility.fieldWidth;
			// Default tag will always be 0 and 'Default'
			EditorGUI.LabelField(new Rect(rt.xMin + padding, rt.yMin, rt.width - padding * 2 - xButtonWidth, rt.height), "Hit Group 0", NSTHitGroupsSettings.DEF_NAME);

			for (int i = 1; i < _target.hitGroupTags.Count; i++)
			{
				rt = EditorGUILayout.GetControlRect();
				EditorGUI.LabelField(new Rect(rt.xMin + padding, rt.yMin, rt.width - padding * 2 - xButtonWidth, rt.height), "Hit Group " + i);
				_target.hitGroupTags[i] = EditorGUI.TextField(new Rect(fieldLeft, rt.yMin, fieldWidth, rt.height), GUIContent.none, _target.hitGroupTags[i]);

				bool isRepeat = IsTagAlreadyUsed(_target.hitGroupTags[i], i);

				if (isRepeat)
				{
					EditorUtils.CreateErrorIconF(EditorGUIUtility.labelWidth - 2, rt.yMin,
						"Each name can only be used once, repeats will be discarded at build time which cause some unpedictable results when looking up by name.");
				}

				if (GUI.Button(new Rect(rt.xMin + rt.width - xButtonWidth - padding, rt.yMin, xButtonWidth, rt.height), "X"))
				{
					_target.hitGroupTags.RemoveAt(i);
				}

			}

			rt = EditorGUILayout.GetControlRect();

			if (_target.hitGroupTags.Count < 32)
				if (GUI.Button(new Rect(rt.xMin + 8, rt.yMin + 3, rt.width - 14, rt.height + 4), "Add Hitbox Group"))
				{
					string newtag = "HitGroup" + _target.hitGroupTags.Count;

					while (IsTagAlreadyUsed(newtag, _target.hitGroupTags.Count))
						newtag += "X";

					_target.hitGroupTags.Add(newtag);
				}

			rt = EditorGUILayout.GetControlRect();

			EditorGUILayout.EndVertical();

			EditorGUILayout.HelpBox(
				"These tags are used by NSTHitboxGroupTag to assign colliders to hitbox groups, for things like headshots and critical hits.", MessageType.None);
			EditorGUILayout.HelpBox(BitTools.BitsNeeded((uint)(_target.hitGroupTags.Count - 1)) + " bits per Rewind Cast added for hit tags.", MessageType.None);
		}

		private bool IsTagAlreadyUsed(string tag, int countUpTo)
		{
			for (int i = 0; i < countUpTo; i++)
				if (_target.hitGroupTags[i] == tag)
					return true;

			return false;
		}

		public static void DrawLinkToSettings()
		{
			Rect r = EditorGUILayout.GetControlRect(false ,48f);

			GUI.Box(r, GUIContent.none, (GUIStyle)"HelpBox");

			float padding = 4;
			float line = r.yMin + padding;
			float width = r.width - padding * 2;

			GUI.Label(new Rect(r.xMin + padding, line, width, 16), "Add/Remove Hit Box Groups here:", (GUIStyle)"MiniLabel");
			line += 18;
			//EditorGUI.HelpBox(new Rect(r.xMin, line, r.width, 56), "Add/Remove hitbox layer tags in 'NSTHitGroupsSettings'.", MessageType.None);
			//line += 27;
			if (GUI.Button(new Rect(r.xMin + padding, line, width, 23), new GUIContent("NSTHitGroupsSettings")))
			{
				Selection.activeGameObject = NSTHitGroupsSettings.EnsureExistsInScene(NSTSettings.DEFAULT_GO_NAME).gameObject;
			}
		}
	}

#endif
}
