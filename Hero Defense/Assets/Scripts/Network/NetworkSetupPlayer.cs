using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkSetupPlayer : NetworkSetup
{

    //public bool localPlayer = false;

    // Use this for initialization
    void Start()
    {
        base.DisableComponents();

        if (isLocalPlayer)
        {
            if (isServer)
            {
                transform.name = "LocalPlayer as Server";
            }
            else
            {
                transform.name = "LocalPlayer as Client";
            }

        }
        else
        {
            if (isClient)
            {
                transform.name = "NetworkPlayer as Server";
            }
            else
            {
                transform.name = "NetworkPlayer as Client";
            }
        }



        if (isLocalPlayer)
        {
            //localPlayer = true;

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
