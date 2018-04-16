using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Networking;

public class CharacterAnimator : MonoBehaviour
{

    const float locomotionAnimationSmoothTime = .1f;

    NavMeshAgent agent;
    Animator animator;

    CharacterCombat characterCombat;

    PlayerMotor motor;

    NetworkAnimator netAnimator;

    //PlayerController playerController;        //Wird garnicht genutzt?

    float speedPercent;

    [HideInInspector]
    public bool isMovedByAgent = true;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        motor = GetComponent<PlayerMotor>();
        characterCombat = GetComponent<CharacterCombat>();
        //playerController = GetComponent<PlayerController>();
        netAnimator = GetComponent<NetworkAnimator>();

        characterCombat.OnAttack += StartAttackAnimation;
        motor.OnPlayerMoved += StopAttackAnimation;
        characterCombat.OnAttackCanceled += StopAttackAnimation;
    }

    void Update()
    {
        speedPercent = agent.velocity.magnitude / agent.speed;


        animator.SetFloat("speedPercent", speedPercent, locomotionAnimationSmoothTime, Time.deltaTime);
    }

    void StartAttackAnimation()
    {
        animator.SetBool("cancelAttack", false);

        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Shoot"))
        {
            //netAnimator.SetTrigger("attack");
            animator.Play("Shoot", -1, 0f); //Animation wird direkt abgespielt. Würde sie geblended werden, sähe das merkwürdig aus. Warum? Verstehe ich auch nicht...
        }
        else
        {
            //netAnimator.SetTrigger("attack");
            animator.SetTrigger("attack");  //Animation wird Geblendet
        }

        //animator.SetTrigger("attack");
        //netAnimator.SetTrigger("attack");
    }

    void StopAttackAnimation()
    {
        animator.SetBool("cancelAttack", true);
    }

    public float GetSpeedPercent()
    {
        return speedPercent;
    }

    
    public void SetSpeedPercent(float _speedPercent)
    {
        speedPercent = _speedPercent;
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
