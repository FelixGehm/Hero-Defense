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

    public Texture2D friendlyTargetCursor;

    PlayerMotor motor;

    private bool targetClicked = false;
    private Vector3 targetPosition;
    private string targetID;

    public event System.Action OnCasting;

    // Use this for initialization
    protected override void Start()
    {
        base.Start();
        motor = GetComponent<PlayerMotor>();
        //motor.OnPlayerMoved += () => targetClicked = false;
        //motor.OnFollowTarget += () => targetClicked = false;

        //motor.OnPlayerMoved += DisableAbility;
        //motor.OnFollowTarget += DisableAbility;

        GetComponent<CharacterEventManager>().OnRevive += ActivateAbility;
    }


    protected override void Update()
    {
        base.Update();
        //Debug.Log(currentCooldown);

        if (IsAbilityActivated && Input.GetMouseButtonDown(0))
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

        if(IsAbilityActivated && Input.GetMouseButtonDown(1))
        {
            DisableAbility();
        }

        if (targetClicked)
        {
            if (Input.GetMouseButtonDown(1)){
                targetClicked = false;
                DisableAbility();
            }

            float distanceToTarget = Vector3.Distance(targetPosition, transform.position);
            if (distanceToTarget <= reviveDistance)
            {
                motor.StopMoving();
                StartCoroutine(CastAfterDelay(abilityCastTime));
            }
        }
    }

    private bool isAbilityActivated; //just activated, not casted yet
    public bool IsAbilityActivated
    {
        get
        {
            return isAbilityActivated;
        }
        set
        {
            isAbilityActivated = value;
            if (value)
            {
                //set mouse indicator to revive
                Cursor.SetCursor(friendlyTargetCursor, Vector2.zero, CursorMode.Auto);
            }
            else
            {
                //set mouse indicator to standard
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            }
        }
    }

    private void DisableAbility()  //setzt die ability nur dann auf false; wenn sie true war, um zu verhindern, dass der mauszeiger unnötig oft erneuert wird
    {
        if (IsAbilityActivated) IsAbilityActivated = false;
    }

    private void ActivateAbility()
    {
        if (currentCooldown <= 0) IsAbilityActivated = true;
    }

    void MoveToTarget(Transform target, RaycastHit hit)
    {
        targetPosition = target.position;
        motor.MoveToPoint(targetPosition);
        targetClicked = true;
        IsAbilityActivated = false;
        targetID = hit.collider.name;
    }

    bool IsTargetDead(string _ID)
    {
        return GameObject.Find(_ID).GetComponent<PlayerStats>().SyncedCurrentHealth <= 0;
    }


    IEnumerator CastAfterDelay(float delay)
    {
        targetClicked = false;
        OnCasting?.Invoke();

        Debug.Log("Test");

        yield return new WaitForSeconds(delay);
        Cast();
    }

    [Client]
    protected override void Cast()
    {
        Debug.Log(targetID + " has been revived.");
        CmdRevivePlayer(targetID);
        targetClicked = false;
        currentCooldown = abilityCooldown;
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
