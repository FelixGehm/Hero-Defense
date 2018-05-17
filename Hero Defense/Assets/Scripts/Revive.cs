using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;



//TODO targetClicked beim attacken zurücksetzen


//Gives a player the ability to revive another player

[RequireComponent(typeof(PlayerMotor))]
public class Revive : AbilityBasic
{
    public LayerMask remotePlayerMask;
    public float reviveDistance = 2;

    private bool wlnd;

    PlayerMotor motor;

    private bool targetClicked = false;
    private Vector3 targetPosition;
    private string targetID;



    // Use this for initialization
    protected override void Start()
    {
        base.Start();
        motor = GetComponent<PlayerMotor>();
        motor.OnPlayerMoved += () => targetClicked = false;
        motor.OnFollowTarget += () => targetClicked = false;
    }


    protected override void Update()
    {
        base.Update();
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 100, remotePlayerMask))
            {
                if (hit.collider.tag == "Player" && IsTargetDead(hit.collider.name))
                {
                    MoveToTarget(hit.collider.transform, hit);
                }
            }
        }

        if (targetClicked)
        {
            float distanceToTarget = Vector3.Distance(targetPosition, transform.position);
            if (distanceToTarget <= reviveDistance)
            {
                motor.StopMoving();
                StartCoroutine(CastAfterDelay(abilityCastTime));
            }
        }
    }

    void MoveToTarget(Transform target, RaycastHit hit)
    {
        targetPosition = target.position;
        motor.MoveToPoint(targetPosition);
        targetClicked = true;
        targetID = hit.collider.name;
    }

    bool IsTargetDead(string _ID)
    {
        return GameObject.Find(_ID).GetComponent<PlayerStats>().CurrentHealth <= 0;
    }


    IEnumerator CastAfterDelay(float delay)        //TODO
    {
        yield return new WaitForSeconds(delay);
        Cast();
    }

    [Client]
    protected override void Cast()
    {
        Debug.Log(targetID + " has been revived.");
        CmdRevivePlayer(targetID);
        targetClicked = false;
    }

    [Command]
    void CmdRevivePlayer(string _ID)
    {
        GameObject.Find(_ID).GetComponent<PlayerController>().RevivePlayer();
        RpcRevivePlayer(_ID);
    }

    [ClientRpc]
    void RpcRevivePlayer(string _ID)
    {
        GameObject.Find(_ID).GetComponent<PlayerController>().RevivePlayer();
    }
}
