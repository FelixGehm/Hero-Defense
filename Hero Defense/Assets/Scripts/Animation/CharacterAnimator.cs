﻿using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Networking;

public class CharacterAnimator : MonoBehaviour
{
    //Animationen können über die state machine abgespielt werden, indem die entsprechenden Attribute gesetzt werden (alles wird synchronisiert)
    //Sollen Animationen direkt gestartet werden, muss dies über nsa.play... geschehen, damit alles synchronisiert wird

    public AbilityBasic abilityQ;
    public AbilityBasic abilityW;
    public AbilityBasic abilityE;
    public AbilityBasic abilityR;

    const float locomotionAnimationSmoothTime = .1f;

    NavMeshAgent agent;
    Animator animator;

    CharacterCombat characterCombat;

    PlayerMotor motor;

    NetworkAnimator netAnimator;

    NetworkSyncAnimations nsa;

    CharacterStats myStats;

    PlayerController pc;

    Revive rev;

    //PlayerController playerController;        //Wird garnicht genutzt?

    float speedPercent;

    void Start()
    {
        pc = GetComponent<PlayerController>();
        agent = GetComponent<NavMeshAgent>();
        //animator = GetComponentInChildren<Animator>();
        animator = GetComponent<Animator>();
        motor = GetComponent<PlayerMotor>();
        myStats = GetComponent<CharacterStats>();
        characterCombat = GetComponent<CharacterCombat>();
        //playerController = GetComponent<PlayerController>();
        netAnimator = GetComponent<NetworkAnimator>();
        nsa = GetComponent<NetworkSyncAnimations>();
        rev = GetComponent<Revive>();

        characterCombat.OnAttack += StartAttackAnimation;
        motor.OnPlayerMoved += StopAttackAnimation;
        characterCombat.OnAttackCanceled += StopAttackAnimation;
        //myStats.attackSpeed.OnStatChanged += UpdateAttackAnimationSpeed;
        pc.OnPlayerKilled += StartDieAnimation;
        pc.OnPlayerRevived += () => netAnimator.SetTrigger("revived");

        rev.OnCasting += StartReviveAnimation;


        if (abilityQ != null)   //tmp -> delete if statement after
        {
            abilityQ.OnAbilityCasting += StartQAnimation;
            abilityW.OnAbilityCasting += StartWAnimation;
            abilityW.OnAbilityCancaled += CancelWAnimation;
            abilityE.OnAbilityCasting += StartEAnimation;
            abilityE.OnAbilityCancaled += CancelEAnimation;
            abilityR.OnAbilityCasting += StartRAnimation;
            abilityR.OnAbilitySecondCasting += StartSecondRAnimation;
            abilityR.OnAbilityCancaled += CancelRAnimation;
        }
        else
        {
            //test
            abilityW.OnAbilityCasting += StartWAnimation;
            abilityW.OnAbilityCancaled += CancelWAnimation;
            abilityE.OnAbilityCasting += StartEAnimation;
            abilityE.OnAbilityCancaled += CancelEAnimation;
            abilityR.OnAbilityCasting += StartRAnimation;
            abilityR.OnAbilityCancaled += CancelRAnimation;
            //endtest
        }





        UpdateAttackAnimationSpeed();
    }

    void Update()
    {
        speedPercent = agent.velocity.magnitude / agent.speed;


        animator.SetFloat("speedPercent", speedPercent, locomotionAnimationSmoothTime, Time.deltaTime);
    }

    void TriggerAnimation(string animation)
    {
        netAnimator.SetTrigger(animation);
        animator.ResetTrigger(animation);
    }

    void StartDieAnimation()
    {
        netAnimator.SetTrigger("die");
        animator.ResetTrigger("die");
    }

    void StartReviveAnimation()
    {
        netAnimator.SetTrigger("revive");
        animator.ResetTrigger("revive");
    }

    void StartQAnimation()
    {
        netAnimator.SetTrigger("abilityQ");
        animator.ResetTrigger("abilityQ");
    }

    void StartWAnimation()
    {
        netAnimator.SetTrigger("abilityW");
        animator.ResetTrigger("abilityW");
    }

    void CancelWAnimation()
    {
        netAnimator.SetTrigger("cancelW");
        animator.ResetTrigger("cancelW");
    }

    void StartEAnimation()
    {
        netAnimator.SetTrigger("abilityE");
        animator.ResetTrigger("abilityE");
    }

    void CancelEAnimation()
    {
        if (!IsInAnimation("AbilityE")) return;

        netAnimator.SetTrigger("cancelE");
        animator.ResetTrigger("cancelE");
    }

    void StartRAnimation()
    {
        //Debug.Log("CharacterAnimator: StartRAnimation()");
        //nsa.StartRAnimation();
        netAnimator.SetTrigger("abilityR");
        animator.ResetTrigger("abilityR");
    }

    void StartSecondRAnimation()
    {
        netAnimator.SetTrigger("abilityR2");
        animator.ResetTrigger("abilityR2");
    }

    void CancelRAnimation()
    {
        netAnimator.SetTrigger("cancelR");
        animator.ResetTrigger("cancelR");
    }

    void StartAttackAnimation()
    {
        Debug.Log("StartAttackAnimation()");

        animator.SetBool("cancelAttack", false);

        UpdateAttackAnimationSpeed(); //TODO: Nicht hier, sondern nur dann, wenn sich der AttackSpeeed ändert

        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Shoot"))
        {
            //animator.Play("Shoot", -1, 0f); //Animation wird direkt abgespielt. Würde sie geblended werden, sähe das merkwürdig aus. Warum? Verstehe ich auch nicht...
            nsa.PlayAttackAnimation();
            //animator.SetTrigger("locoWithoutBlend");
            //animator.SetTrigger("attackWithoutBlend");

            //test
            //netAnimator.SetTrigger("attack");
            //if (NetworkServer.active) animator.ResetTrigger("attack");
        }
        else
        {
            //animator.SetTrigger("attack");
            netAnimator.SetTrigger("attack");
            if (NetworkServer.active) animator.ResetTrigger("attack");
            //animator.SetTrigger("attackWithoutBlend");
        }

        //animator.SetTrigger("attack");
        //netAnimator.SetTrigger("attack");
    }

    void StopAttackAnimation()
    {
        animator.SetBool("attack", false);
        animator.SetBool("cancelAttack", true);

        //animator.ResetTrigger("attackWithoutBlend");
    }

    private void UpdateAttackAnimationSpeed()
    {
        float attackCD = 1 / myStats.attackSpeed.GetValue();
        float clipLength = animator.runtimeAnimatorController.animationClips[2].length;    //attack animation muss immer auf 2 liegen!!! der clip lässt sich nicht über den namen finden...

        float animSpeed = clipLength / attackCD;
        animator.SetFloat("attackAnimationSpeed", animSpeed);
    }


    /*
    public bool IsInAttackAnimation()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Shoot"))
            return true;

        return false;
    }
    */

    private bool IsInAnimation(string animationName)
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName(animationName))
        {
            return true;
        }
        return false;
    }

}
