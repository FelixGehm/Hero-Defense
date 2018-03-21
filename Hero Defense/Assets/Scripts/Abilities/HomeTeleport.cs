using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(CharacterEventManager), typeof(PlayerMotor), typeof(PlayerController))]
public class HomeTeleport : MonoBehaviour
{

    private Vector3 teleportPoint;

    private PlayerMotor motor;


    public float castTime = 0.5f;
    public float getCastTime()
    {
        return castTime;
    }

    Coroutine co;       // Reffereenz auf laufende Coroutine für den Fall, dass abgebrochen wird

    void Start()
    {
        GetComponent<CharacterEventManager>().OnTeleport += TeleportHome;
        GetComponent<CharacterEventManager>().OnCastCancel += CancelTeleport;

        motor = GetComponent<PlayerMotor>();

        teleportPoint = GameObject.Find("HomeTeleportPoint").transform.position;
    }



    public void TeleportHome()
    {
       co = StartCoroutine(TeleportAfterDelay(castTime));
    }

    public void CancelTeleport()
    {
        StopCoroutine(co);
    }

    IEnumerator TeleportAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        transform.position = teleportPoint;
        motor.MoveToPoint(teleportPoint);
    }
}
