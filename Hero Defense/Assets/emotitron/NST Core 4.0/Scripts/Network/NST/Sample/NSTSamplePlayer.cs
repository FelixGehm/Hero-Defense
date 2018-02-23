//Copyright 2018, Davin Carten, All rights reserved

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace emotitron.Network.NST.Sample
{
	[AddComponentMenu("NST/Sample Code/NST Sample Player")]

	/// <summary>
	/// Sample code that interacts with the NetworkSyncTransform on a networked object. 
	/// This sample automatically ressurects players after X seconds by monitoring the INstState of the NST.
	/// for finding and managing your player and nonplayer objects.
	/// Note the use of interface callbacks such as OnNstStartLocalPlayer. These interfaces automatically register themselves
	/// with the root NST object, and are called at the indicated time segement when it occurs in the root NST, allowing for better control
	/// of order of execution.
	/// </summary>
	/// 
	public class NSTSamplePlayer : NSTComponent, INstOnStartLocalPlayer, INstOnDestroy, INstState, INstPostInterpolate // NetworkSyncTransform
	{
		public Camera playerCamera;
		public Camera defaultCam;
		public GameObject playerMesh;
		public Renderer rend;

		private GameObject myTeleportButton;
		[HideInInspector] public static NSTSamplePlayer localPlayer;
		private float deathTime;
		private bool alreadyBorn;

		/// <summary>
		/// Called after the NST on this gameobject completes running its OnStartLocalPlayer.
		/// </summary>
		public void OnNstStartLocalPlayer()
		{
			localPlayer = this;
		}

		public void Start()
		{
			// Pretend we get a state update
			OnNstState(nst.State);
		}

		/// <summary>
		/// Called after the NST on this gameobject completes running Awake().
		/// </summary>
		public override void OnNstPostAwake()
		{
			base.OnNstPostAwake();

			// Find the various cameras
			if (playerCamera == null)
				playerCamera = GetComponentInChildren<Camera>(true);

			// Be sure the player cam is off so it doesn't accidently get mistaken for a main camera
			if (playerCamera)
				playerCamera.gameObject.SetActive(false);

			if (defaultCam == null)
				defaultCam = Camera.main;

			if (playerMesh != null)
				rend = playerMesh.GetComponent<MeshRenderer>();
		}

		// Determine alive state AFTER damage is applied (interpolation finished) and BEFORE updates are sent out.
		/// <summary>
		/// Called before the NST on this gameobject sends out its frame update. Only owners send out updates
		/// </summary>
		public void OnNstPostInterpolate()
		{
			// Svr ressurect dead player after 2 seconds
			if (MasterNetAdapter.ServerIsActive)
				if (Time.time - deathTime > 2f && nst.State != State.Alive)
					nst.State = State.Alive;
		}

		private void SetCamera(bool enablePlayerCam)
		{
			if (playerCamera != null)
			{
				playerCamera.gameObject.SetActive(enablePlayerCam);

				if (defaultCam != null)
					defaultCam.gameObject.SetActive(!enablePlayerCam);
			}
		}

		public void OnNstDestroy()
		{
			if (na.IsLocalPlayer)
				SetCamera(false);
		}

		/// <summary>
		/// Callback from the root NST when alive status changes.
		/// </summary>
		/// <param name="state"></param>
		public void OnNstState(State state)
		{
			bool isalive = state.IsAlive();
			// if this is a ressurection, move to a spawn point. Don't move if this is giving birth (first run) - the NM just assigned this location already.
			if (MasterNetAdapter.ServerIsActive && isalive && alreadyBorn)
				nst.Teleport(MasterNetAdapter.GetPlayerSpawnPoint());

			if (playerMesh != null)
				playerMesh.SetActive(state.IsVisible());

			if (na.IsLocalPlayer)
				SetCamera(isalive);

			// note time of death for respawn methods
			if (isalive == false)
				deathTime = Time.time;

			// note that this object now exists in the world, and will need to be moved to spawn points when it ressurects.
			if (isalive)
				alreadyBorn = true;
		}


#if UNITY_EDITOR

		[CustomEditor(typeof(NSTSamplePlayer))]
		[CanEditMultipleObjects]
		public class NSTSamplePlayerEditor : NSTSampleHeader
		{

		}
#endif

	}
}

