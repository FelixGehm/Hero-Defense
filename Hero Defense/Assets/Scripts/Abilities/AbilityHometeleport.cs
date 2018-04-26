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
    CharacterEventController cec;

    PlayerMotor motor;
    
    void Start()
    {        
        NST = GetComponent<NetworkSyncTransform>();

        characterEventManager = GetComponent<CharacterEventManager>();
        cec = GetComponent<CharacterEventController>();

        teleportPoint = GameObject.Find("HomeTeleportPoint").transform.position;
        motor = GetComponent<PlayerMotor>();

        characterEventManager.OnTeleport += TeleportHome;
    }

    private void Update()
    {
        if (teleport!= null && Input.GetMouseButtonDown(1))        // RightClick
        {
            CancelTeleport();
        }
    }

    Coroutine teleport;
    void TeleportHome()
    {
        teleport = StartCoroutine(TeleportAfterDelay(castTime));
        motor.MoveToPoint(transform.position);
    }

    public float castTime = 0.5f;
    IEnumerator TeleportAfterDelay(float delay)
    {
        cec.isCasting = true;
        yield return new WaitForSeconds(delay);
        NST.Teleport(teleportPoint, transform.rotation);
        motor.MoveToPoint(teleportPoint);
        cec.isCasting = false;
    }

    void CancelTeleport()
    {
        StopCoroutine(teleport);
        cec.isCasting = false;
    }
}
