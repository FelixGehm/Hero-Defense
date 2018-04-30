using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Networking;

public class CharacterAnimator : MonoBehaviour
{
    //Animationen können über die state machine abgespielt werden, indem die entsprechenden Attribute gesetzt werden (alles wird synchronisiert)
    //Sollen Animationen direkt gestartet werden, muss dies über nsa.play... geschehen, damit alles synchronisiert wird

    const float locomotionAnimationSmoothTime = .1f;

    NavMeshAgent agent;
    Animator animator;

    CharacterCombat characterCombat;

    PlayerMotor motor;

    NetworkAnimator netAnimator;

    NetworkSyncAnimations nsa;

    CharacterStats myStats;

    //PlayerController playerController;        //Wird garnicht genutzt?

    float speedPercent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        //animator = GetComponentInChildren<Animator>();
        animator = GetComponent<Animator>();
        motor = GetComponent<PlayerMotor>();
        myStats = GetComponent<CharacterStats>();
        characterCombat = GetComponent<CharacterCombat>();
        //playerController = GetComponent<PlayerController>();
        netAnimator = GetComponent<NetworkAnimator>();
        nsa = GetComponent<NetworkSyncAnimations>();

        characterCombat.OnAttack += StartAttackAnimation;
        motor.OnPlayerMoved += StopAttackAnimation;
        characterCombat.OnAttackCanceled += StopAttackAnimation;
        //myStats.attackSpeed.OnStatChanged += UpdateAttackAnimationSpeed;

        UpdateAttackAnimationSpeed();
    }

    void Update()
    {
        speedPercent = agent.velocity.magnitude / agent.speed;


        animator.SetFloat("speedPercent", speedPercent, locomotionAnimationSmoothTime, Time.deltaTime);
    }

    void StartAttackAnimation()
    {
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

}
