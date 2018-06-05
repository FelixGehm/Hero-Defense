using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkSetupPlayer : NetworkSetup
{

    //public bool localPlayer = false;
    [SerializeField]
    string remoteLayerName = "RemotePlayer";

    // Use this for initialization
    void Start()
    {
        base.DisableComponents();

        /*
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
        */
        if(isServer)
        {
            // Register Player in PlayerManager
            PlayerManager.instance.RegisterPlayer(transform.gameObject);
        }


        if (isLocalPlayer)
        {
            // Bind Camera to Player
            Camera.main.GetComponent<CameraController>().SetLookAt(transform);
        }
        else
        {
            AssignRemoteLayer(); //Alle Remote Player werden auf einen layer gesetzt. z.b. beim reviven können dann nur diese abgefragt werden.
        }

        //Give Every Player a unique ID as its name
        string _ID = "Player " + GetComponent<NetworkIdentity>().netId;
        transform.name = _ID;

    }

    void AssignRemoteLayer()
    {
        gameObject.layer = LayerMask.NameToLayer(remoteLayerName);
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        GetComponent<NetworkAnimator>().SetParameterAutoSend(0, true);
        GetComponent<NetworkAnimator>().SetParameterAutoSend(1, true);
        GetComponent<NetworkAnimator>().SetParameterAutoSend(2, true);
        GetComponent<NetworkAnimator>().SetParameterAutoSend(3, true);

        GetComponent<NetworkAnimator>().SetParameterAutoSend(4, true);
        GetComponent<NetworkAnimator>().SetParameterAutoSend(5, true);
        GetComponent<NetworkAnimator>().SetParameterAutoSend(6, true);
        GetComponent<NetworkAnimator>().SetParameterAutoSend(7, true);
        GetComponent<NetworkAnimator>().SetParameterAutoSend(8, true);
        GetComponent<NetworkAnimator>().SetParameterAutoSend(9, true);
        GetComponent<NetworkAnimator>().SetParameterAutoSend(10, true);
        GetComponent<NetworkAnimator>().SetParameterAutoSend(11, true);
    }

    public override void PreStartClient()
    {
        base.PreStartClient();
        GetComponent<NetworkAnimator>().SetParameterAutoSend(0, true);
        GetComponent<NetworkAnimator>().SetParameterAutoSend(1, true);
        GetComponent<NetworkAnimator>().SetParameterAutoSend(2, true);
        GetComponent<NetworkAnimator>().SetParameterAutoSend(3, true);

        GetComponent<NetworkAnimator>().SetParameterAutoSend(4, true);
        GetComponent<NetworkAnimator>().SetParameterAutoSend(5, true);
        GetComponent<NetworkAnimator>().SetParameterAutoSend(6, true);
        GetComponent<NetworkAnimator>().SetParameterAutoSend(7, true);
        GetComponent<NetworkAnimator>().SetParameterAutoSend(8, true);
        GetComponent<NetworkAnimator>().SetParameterAutoSend(9, true);
        GetComponent<NetworkAnimator>().SetParameterAutoSend(10, true);
        GetComponent<NetworkAnimator>().SetParameterAutoSend(11, true);
    }

    private void OnDestroy()
    {
        // Remove Player from PlayerManager
        PlayerManager.instance.RemovePlayer(transform.gameObject);
    }


}
