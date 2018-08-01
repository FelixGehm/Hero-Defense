using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Networking;

public class EnemyAnimator : MonoBehaviour
{

    NavMeshAgent agent;
    Animator animator;
    NetworkAnimator netAnimator;
    CharacterCombat characterCombat;

    const float locomotionAnimationSmoothTime = .1f;
    float speedPercent;
    // Use this for initialization
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        netAnimator = GetComponent<NetworkAnimator>();
        characterCombat = GetComponent<CharacterCombat>();
        characterCombat.OnAttack += StartAttackAnimation;
    }

    // Update is called once per frame
    void Update()
    {
        speedPercent = agent.velocity.magnitude / agent.speed;

        animator.SetFloat("speedPercent", speedPercent, locomotionAnimationSmoothTime, Time.deltaTime);
    }

    void StartAttackAnimation()
    {
        //animator.SetTrigger("attack");

        netAnimator.SetTrigger("attack");
        if (NetworkServer.active) animator.ResetTrigger("attack");
    }

    public void StartMortarAnimation()
    {
        netAnimator.SetTrigger("mortar");
        if (NetworkServer.active) animator.ResetTrigger("mortar");
    }
}
