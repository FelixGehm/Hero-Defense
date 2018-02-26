using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.AI;

public class CharacterAnimator : NetworkBehaviour
{

    const float locomotionAnimationSmoothTime = .1f;

    NavMeshAgent agent;
    Animator animator;
    CharacterCombat characterCombat;
    PlayerController playerController;

    [SyncVar]
    float speedPercent;



    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();

        characterCombat = GetComponent<CharacterCombat>();
        characterCombat.OnAttack += StartAttackingAnimation;

        playerController = GetComponent<PlayerController>();
        
    }


    void Update()
    {

        //if (!isLocalPlayer) { return; }

        if (isLocalPlayer)
        {
            speedPercent = agent.velocity.magnitude / agent.speed;
            CmdSetSpeedPercent(speedPercent);
        }
         
        
        animator.SetFloat("speedPercent", speedPercent, locomotionAnimationSmoothTime, Time.deltaTime);
    }

    void StartAttackingAnimation()
    {
        animator.SetBool("attackBool", true);
        playerController.focus.OnDefocus += StopAttackAnimation;
    }

    void StopAttackAnimation()
    {
        animator.SetBool("attackBool", false);
    }

    [Command]
    void CmdSetSpeedPercent(float speed)
    {
        speedPercent = speed;
    }
}
