using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkSetupPlayer : NetworkSetup {

	// Use this for initialization
	void Start ()
    {
        base.DisableComponents();

        if (isLocalPlayer)
        {
            // Bind Camera to Player
            Camera.main.GetComponent<CameraController>().SetLookAt(transform);

            // Register Player in PlayerManager
            PlayerManager.instance.RegisterPlayer(transform.gameObject);
        }
            
    }

    private void OnDestroy()
    {
        // Remove Player from PlayerManager
        PlayerManager.instance.RemovePlayer(transform.gameObject);
    }


}
