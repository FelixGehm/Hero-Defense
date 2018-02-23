//Copyright 2018, Davin Carten, All rights reserved

using UnityEngine;


namespace emotitron
{

	public abstract class Singleton<T> : MonoBehaviour where T : Component
	{
		public static T single;
		protected static bool isShuttingDown;

		protected virtual void Awake()
		{
			isShuttingDown = false;

			if (single != null && single != this)
			{
				Debug.LogWarning("Enforcing " + typeof(T) + " singleton. Multiples found.");
				Destroy(this);
			}
			single = this as T;
		}

		protected virtual void OnApplicationQuit()
		{
			isShuttingDown = true;
		}

		/// <summary>
		/// Call this at the awake of other NST components to make sure that NSTSettings exists in the scene.
		/// </summary>
		public static T EnsureExistsInScene(string goName, bool isExpanded = true)
		{

			if (single != null)
				return single;

			single = FindObjectOfType<T>();

			if (single != null)
				return single;

			// Don't attempt to make a new one if we are shutting down a game.
			if (isShuttingDown && Application.isPlaying)
				return single;

			DebugX.LogWarning(!DebugX.logWarnings ? "" : ("<b>No " + (typeof(T)) + " found in scene. Adding one with default settings.</b> You probably want to edit the settings yourself."));

			GameObject go = GameObject.Find(goName);

			if (go == null)
				go = new GameObject(goName);

			single = go.AddComponent<T>();

#if UNITY_EDITOR

			UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(single, isExpanded);
#endif

			return single;
		}

	}
}

