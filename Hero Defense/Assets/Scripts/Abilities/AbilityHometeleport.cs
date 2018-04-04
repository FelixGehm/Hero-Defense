using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using emotitron.Network.NST;



[RequireComponent(typeof(NetworkSyncTransform), typeof(CharacterEventManager), typeof(PlayerMotor))]
public class AbilityHometeleport : NetworkBehaviour
{
    private Vector3 teleportPoint;

    NetworkSyncTransform NST;
    CharacterEventManager characterEventManager;
    PlayerMotor motor;
    
    void Start()
    {        
        NST = GetComponent<NetworkSyncTransform>();

        characterEventManager = GetComponent<CharacterEventManager>();

        teleportPoint = GameObject.Find("HomeTeleportPoint").transform.position;
        motor = GetComponent<PlayerMotor>();

        //if (isServer)
        {
            characterEventManager.OnTeleport += TeleportHome;
        }
    }

    void TeleportHome()
    {
        StartCoroutine(TeleportAfterDelay(castTime));
    }

    public float castTime = 0.5f;
    IEnumerator TeleportAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        NST.Teleport(teleportPoint, transform.rotation, true);
        motor.MoveToPoint(teleportPoint);
    }
}
