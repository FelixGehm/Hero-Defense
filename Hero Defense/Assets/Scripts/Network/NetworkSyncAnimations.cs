using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkSyncAnimations : NetworkBehaviour
{
    [SyncVar]
    float speedPercent;



    /*
    [SyncVar]
    bool isAttacking = false;
    */
    Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();

        if (!isLocalPlayer)
        {
            //characterAnimator.isMovedByAgent = false;
        }

    }

    void Update()
    {
        if (isLocalPlayer)
        {
            //speedPercent = characterAnimator.GetSpeedPercent();
            //CmdSetSpeedPercent(characterAnimator.GetSpeedPercent());
        }
        else
        {
            //characterAnimator.SetSpeedPercent(speedPercent);
        }
    }

    public void PlayAttackAnimation()
    {
        animator.Play("Shoot", -1, 0f);
        CmdSyncAttackAnimation();
    }

    [Command]
    private void CmdSyncAttackAnimation()
    {
        animator.Play("Shoot", -1, 0f);
        RpcSyncAttackAnimation();
    }

    [ClientRpc]
    private void RpcSyncAttackAnimation()
    {
        animator.Play("Shoot", -1, 0f);
    }

    [Command]
    void CmdSetSpeedPercent(float speed)
    {
        //characterAnimator.SetSpeedPercent(speed);
    }


}
