using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.Networking;
using UnityEngine.AI;

public class CharacterAnimator : MonoBehaviour
{

    const float locomotionAnimationSmoothTime = .1f;

    NavMeshAgent agent;
    Animator animator;
    CharacterCombat characterCombat;
    PlayerController playerController;

    float speedPercent;

    [HideInInspector]
    public bool isMovedByAgent = true;

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

        if (isMovedByAgent)
        {
            speedPercent = agent.velocity.magnitude / agent.speed;
        }
        //Debug.Log(speedPercent);

        animator.SetFloat("speedPercent", speedPercent, locomotionAnimationSmoothTime, Time.deltaTime);
    }

    void StartAttackingAnimation()
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
