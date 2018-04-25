using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

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

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        GetComponent<NetworkAnimator>().SetParameterAutoSend(0, true);
        GetComponent<NetworkAnimator>().SetParameterAutoSend(1, true);
        GetComponent<NetworkAnimator>().SetParameterAutoSend(2, true);
        GetComponent<NetworkAnimator>().SetParameterAutoSend(3, true);
    }

    public override void PreStartClient()
    {
        base.PreStartClient();
        GetComponent<NetworkAnimator>().SetParameterAutoSend(0, true);
        GetComponent<NetworkAnimator>().SetParameterAutoSend(1, true);
        GetComponent<NetworkAnimator>().SetParameterAutoSend(2, true);
        GetComponent<NetworkAnimator>().SetParameterAutoSend(3, true);
    }

    private void OnDestroy()
    {
        // Remove Player from PlayerManager
        PlayerManager.instance.RemovePlayer(transform.gameObject);
    }


}
