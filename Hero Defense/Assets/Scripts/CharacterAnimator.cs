﻿using UnityEngine;
using UnityEngine.AI;

public class CharacterAnimator : MonoBehaviour
{

    const float locomotionAnimationSmoothTime = .1f;

    NavMeshAgent agent;
    Animator animator;

    CharacterCombat characterCombat;
    NetworkCharacterCombat networkCharacterCombat;

    PlayerController playerController;

    float speedPercent;

    [HideInInspector]
    public bool isMovedByAgent = true;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();

        if (GetComponent<CharacterCombat>() == null)
        {
            networkCharacterCombat = GetComponent<NetworkCharacterCombat>();
            networkCharacterCombat.OnAttack += StartAttackAnimation;
        }
        else
        {
            characterCombat = GetComponent<CharacterCombat>();
            characterCombat.OnAttack += StartAttackAnimation;
        }

    
        playerController = GetComponent<PlayerController>();
        playerController.OnFocusNull += StopAttackAnimation;

    }

    void Update()
    {

        if (isMovedByAgent)
        {
            speedPercent = agent.velocity.magnitude / agent.speed;
        }        

        animator.SetFloat("speedPercent", speedPercent, locomotionAnimationSmoothTime, Time.deltaTime);
    }

    void StartAttackAnimation()
    {
        animator.SetBool("attackBool", true);
        animator.SetTrigger("attack");
        playerController.focus.OnDefocus += StopAttackAnimation;
    }

    void StopAttackAnimation()
    {
        animator.SetBool("attackBool", false);
    }

    public float GetSpeedPercent()
    {
        return speedPercent;
    }

    public void SetSpeedPercent(float _speedPercent)
    {
        speedPercent = _speedPercent;
    }
}
