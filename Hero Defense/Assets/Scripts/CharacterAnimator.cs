using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CharacterAnimator : MonoBehaviour
{

    const float locomotionAnimationSmoothTime = .1f;

    NavMeshAgent agent;
    Animator animator;
    CharacterCombat characterCombat;
    PlayerController playerController;



    // Use this for initialization
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();

        characterCombat = GetComponent<CharacterCombat>();
        characterCombat.OnAttack += StartAttackingAnimation;

        playerController = GetComponent<PlayerController>();
        
    }

    // Update is called once per frame
    void Update()
    {
        float speedPercent = agent.velocity.magnitude / agent.speed;
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
}
