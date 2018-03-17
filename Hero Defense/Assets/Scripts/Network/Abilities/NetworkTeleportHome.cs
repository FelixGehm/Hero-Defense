using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using emotitron.Network.NST;



[RequireComponent(typeof(HomeTeleport), typeof(NetworkSyncTransform), typeof(CharacterEventManager))]
public class NetworkTeleportHome : NetworkBehaviour
{
    HomeTeleport homeTeleport;
    NetworkSyncTransform NST;
    CharacterEventManager characterEventManager;
    
    void Start()
    {
        homeTeleport = GetComponent<HomeTeleport>();

        castTime = homeTeleport.getCastTime();

        NST = GetComponent<NetworkSyncTransform>();

        characterEventManager = GetComponent<CharacterEventManager>();

        if (isServer)
        {
            characterEventManager.OnTeleport += TeleportHome;
        }
    }

    void TeleportHome()
    {
        StartCoroutine(TeleportAfterDelay(castTime));
    }

    float castTime;
    IEnumerator TeleportAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        NST.Teleport(homeTeleport.teleportPoint, transform.rotation, true);
    }
}
