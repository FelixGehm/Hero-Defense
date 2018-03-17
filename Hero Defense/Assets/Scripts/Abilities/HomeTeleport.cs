using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(CharacterEventManager), typeof(PlayerMotor), typeof(PlayerController))]
public class HomeTeleport : MonoBehaviour {
    
    public Vector3 teleportPoint;

    private PlayerMotor motor;
  

    public float castTime = 0.5f;
    public float getCastTime()
    {
        return castTime;
    }

    private bool isCasting = false;

	void Start () {
        GetComponent<CharacterEventManager>().OnTeleport += TeleportHome;
        GetComponent<CharacterEventManager>().OnCastCancel += CancelTeleport;
        
        motor = GetComponent<PlayerMotor>();
    
        teleportPoint = GameObject.Find("HomeTeleportPoint").transform.position;
	}

    

    public void TeleportHome()
    {
        //Debug.Log("TeleportHome():");
        StartCoroutine(TeleportAfterDelay(castTime));
    }

    public void CancelTeleport()
    {
        isCasting = false;
    }

    IEnumerator TeleportAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);        
        transform.position = teleportPoint;        
        motor.MoveToPoint(teleportPoint);
    }
}
