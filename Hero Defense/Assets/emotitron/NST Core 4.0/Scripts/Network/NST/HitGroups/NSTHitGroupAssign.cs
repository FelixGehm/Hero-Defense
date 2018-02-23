using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//Copyright 2018, Davin Carten, All rights reserved

using emotitron.Network.NST.Rewind;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace emotitron.Network.NST
{
	/// <summary>
	/// Attach this to objects with colliders. This tells the NST Rewind engine which rewind layer colliers on this GO (and its children if checked) belong to.
	/// </summary>
	public class NSTHitGroupAssign : NSTComponent, IIncludeOnGhost
	{
		public HitGroupSelector hitGroupSelector;
		
		public bool applyToChildren;

		public override void OnNstPostAwake()
		{
			base.OnNstPostAwake();
			hitGroupSelector.ValidateSelection(this);

			// Copy this hitgroup to all relevant children GOs with colliders
			if (applyToChildren)
				CloneToAllChildrenWithColliders(transform, this);

		}

		// if applyToChildren is checked, this HitGroup component needs to be copied to all applicable gameobjects with colliders
		private void CloneToAllChildrenWithColliders(Transform par, NSTHitGroupAssign parNstHitGroup)
		{
			if (!applyToChildren)
				return;

			for (int i = 0; i < par.childCount; i++)
			{
				Transform child = par.GetChild(i);

				// if this child has its own NSTHitGroup with applyToChildren = true then stop recursing this branch, that hg will handle that branch.
				NSTHitGroupAssign hg = child.GetComponent<NSTHitGroupAssign>();
				if (hg != null && hg.applyToChildren)
					continue;

				// Copy the parent NSTHitGroup to this child if it has a collider and no NSTHitGroup of its own
				if (hg == null && child.GetComponent<Collider>() != null)
					parNstHitGroup.ComponentCopy(child.gameObject);

				// recurse this on its children
				CloneToAllChildrenWithColliders(child, parNstHitGroup);
			}
		}

		//protected override void Awake()
		//{
		//	base.Awake();
		//	//hitGroupSelector.ValidateSelection(this);
		//}
		private void Reset()
		{
			NSTHitGroupsSettings.EnsureExistsInScene(NSTSettings.DEFAULT_GO_NAME);
		}

		public override string ToString()
		{
			return base.ToString() + hitGroupSelector;
		}
	}

#if UNITY_EDITOR

	[CustomEditor(typeof(NSTHitGroupAssign))]
	[CanEditMultipleObjects]
	public class NSTHitGroupEditor : NSTHeaderEditorBase
	{
		NSTHitGroupAssign nstSetTag;

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			NSTHitGroupsSettingsEditor.DrawLinkToSettings();
			//EditorGUILayout.HelpBox("The selected tag can be used by NSTCastDefinition to group hitboxes into categories, for critical hits and other zone damage.", MessageType.None);
		}
	}
#endif
}


