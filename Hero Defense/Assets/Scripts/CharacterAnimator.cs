using UnityEngine;
using UnityEngine.AI;

public class CharacterAnimator : MonoBehaviour
{

    const float locomotionAnimationSmoothTime = .1f;

    NavMeshAgent agent;
    Animator animator;

    CharacterCombat characterCombat;

    PlayerMotor motor;

    PlayerController playerController;

    float speedPercent;

    [HideInInspector]
    public bool isMovedByAgent = true;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        motor = GetComponent<PlayerMotor>();
        characterCombat = GetComponent<CharacterCombat>();
        playerController = GetComponent<PlayerController>();

        characterCombat.OnAttack += StartAttackAnimation;
        motor.OnPlayerMoved += StopAttackAnimation;
        //characterCombat.OnAttackCanceled += StopAttackAnimation;
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
        animator.SetBool("cancelAttack", false);
        animator.SetTrigger("attack");
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

    public bool IsInAttackAnimation()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Shoot"))
            return true;

        return false;
    }
}
