using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerSyncTransform : NetworkBehaviour {


    public float lerpRate = 10;

    [SyncVar]
    Quaternion syncRotation;


    [SyncVar]
    private Vector3 syncPos;

    public float tresholdPos = 0.5f;
    private Vector3 lastPosition;

    public float tresholdRot = 5;
    private Quaternion lastRotation;

    private void Update()
    {
        TransmitPosition();
        LerpPosition();
        TransmitRotaion();
        LerpRotation();
    }

    //position
    void LerpPosition()
    {
        if (!isLocalPlayer)
        {
            transform.position = Vector3.Lerp(transform.position, syncPos, Time.deltaTime * lerpRate);
        }
    }

    [Command]
    void CmdProvidePositionToServer(Vector3 pos)
    {
        syncPos = pos;
    }

    [ClientCallback]
    void TransmitPosition()
    {
        if (isLocalPlayer && Vector3.Distance(transform.position, lastPosition) > tresholdPos )
        {
            CmdProvidePositionToServer(transform.position);

            lastPosition = transform.position;
        }
    }

    //rotation
    void LerpRotation()
    {
        if (!isLocalPlayer)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, syncRotation, Time.deltaTime * lerpRate);
        }
    }

    [Command]
    void CmdProvideRotationToServer(Quaternion playerRot)
    {
        syncRotation = playerRot;
    }

    [ClientCallback]
    void TransmitRotaion()
    {
        if (isLocalPlayer )
        {
            if ( Quaternion.Angle(transform.rotation, lastRotation) > tresholdRot)
            {
                CmdProvideRotationToServer(transform.rotation);

                lastRotation = transform.rotation;
            }
        }
    }


    
	
}
