//Copyright 2018, Davin Carten, All rights reserved

using UnityEngine;
using System.Collections.Generic;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR
namespace emotitron.Utilities.GUIUtilities
{
	public static class EditorUtils
	{


		public static void CreateErrorIconF(float xmin, float ymin, string _tooltip)
		{
			GUIContent errorIcon = EditorGUIUtility.IconContent("CollabError");
			errorIcon.tooltip = _tooltip;
			EditorGUI.LabelField(new Rect(xmin, ymin, 16, 16), errorIcon);

		}

		/// <summary>
		/// If this gameobject is a clone of a prefab, will return that prefab source. Otherwise just returns the go that was supplied.
		/// </summary>
		public static GameObject GetPrefabSourceOfGameObject(this GameObject go)
		{
			return (PrefabUtility.GetPrefabParent(go) as GameObject);
		}

		/// <summary>
		/// A not so efficient find of all instances of a prefab in a scene.
		/// </summary>
		public static List<GameObject> FindAllPrefabInstances(GameObject myPrefab)
		{ 
			List<GameObject> result = new List<GameObject>();
			GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
			foreach (GameObject GO in allObjects)
			{
				if (PrefabUtility.GetPrefabType(GO) == PrefabType.PrefabInstance)
				{
					UnityEngine.Object GO_prefab = PrefabUtility.GetPrefabParent(GO);
					if (myPrefab == GO_prefab)
						result.Add(GO);
				}
			}
			return result;
		}

		/// <summary>
		/// Add missing components, but not to a clone if the source prefab already has one - it will cause conflicts.
		/// </summary>
		public static T EnsureRootComponentExists<T>(this GameObject selectedGO, bool isExpanded = true) where T : Component
		{
			bool isPrefab = PrefabUtility.GetPrefabType(selectedGO) == PrefabType.Prefab;
			bool isPrefabInstance = PrefabUtility.GetPrefabType(selectedGO) == PrefabType.PrefabInstance;

			// Add the adapter to the source prefab if one exists, rather than the clone.
			GameObject srcGO = selectedGO.GetPrefabSourceOfGameObject();

			return EnsureRootComponentExists<T>(selectedGO, srcGO, isPrefab, isPrefabInstance, isExpanded);
		}

		/// <summary>
		/// Add missing components, Ideally adds to the prefab master, so it appears on any scene versions and doesn't require Apply.
		/// </summary>
		public static T EnsureRootComponentExists<T>(this GameObject selectedGO, GameObject srcGO, bool isPrefab, bool isPrefabInstance, bool isExpanded = true) where T : Component
		{
			T selected_t = selectedGO.GetComponent<T>();

			if (isPrefab)
			{
				// Remove the NI from a scene object before we add it to the prefab
				if (selected_t == null)
				{
					List<GameObject> clones = FindAllPrefabInstances(selectedGO);

					// Delete all instances of this root component in all instances of the prefab, so when we add to the prefab source they all get it.
					foreach (GameObject clone in clones)
					{
						T[] comp = clone.GetComponents<T>();
						foreach (T t in comp)
							GameObject.DestroyImmediate(t);
					}
					selected_t = selectedGO.AddComponent<T>();
				}
			}
			else if (isPrefabInstance)
			{
				T src_t = (srcGO) ? srcGO.GetComponent<T>() : null;

				// If this component is also missing from the source prefab, add it there (will appear on scene copy)
				if (!src_t)
				{
					if(selected_t == null)
					{
						selected_t = selectedGO.AddComponent<T>();
					}
				} 
			}
			// this is has no prefab source
			else
			{
				if (selected_t == null)
				{
					selected_t = selectedGO.AddComponent<T>();
				}
			}

			if (selected_t)
				UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(selected_t, isExpanded);

			return selected_t;
		}

	}
}
#endif


