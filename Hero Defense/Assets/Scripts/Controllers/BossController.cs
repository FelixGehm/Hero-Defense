


using UnityEngine;

public class BossController : EnemyController
{

    private float attackRange;
    public float mortarRange;
    public float bodySlamRange;
    private BossCombat bCombat;
    public override void Awake()
    {
        base.Awake();
        bCombat = GetComponent<BossCombat>();
        attackRange = GetComponent<EnemyStats>().attackRange.GetValue();
    }

    protected override void Update()
    {
        if (myStatuses.Contains(Status.stunned))
        {
            // Tue nichts, solange bis der Stun vorbei ist.
            return;
        }

        if (!myStatuses.Contains(Status.taunted))
        {
            targetTransform = GetTarget();
        }

        //Moving to Player and attack
        if (!bCombat.isAttacking && !bCombat.isFiringMortar && !bCombat.IsBodySlamming && !bCombat.IsFiringCanons)
            agent.SetDestination(targetTransform.position);

        if (distanceToTarget <= attackRange)
        {

            if (!bCombat.isFiringMortar && !bCombat.IsBodySlamming && !bCombat.IsFiringCanons)
            {
                agent.ResetPath();
                CharacterStats targetStats = targetTransform.GetComponent<CharacterStats>();

                if (targetStats != null)
                {
                    bCombat.Attack(targetStats);
                }

                FaceTarget(targetTransform);
            }

        }

        if (distanceToTarget <= mortarRange && !bCombat.isAttacking && !bCombat.IsBodySlamming && !bCombat.IsFiringCanons && bCombat.IsMortarReady)
        {
            agent.ResetPath();
            CharacterStats targetStats = targetTransform.GetComponent<CharacterStats>();

            if (targetStats != null)
            {
                bCombat.MortarAttack(targetTransform.position);
            }

            FaceTarget(targetTransform);
        }

        if (distanceToTarget <= bodySlamRange && !bCombat.isAttacking && !bCombat.isFiringMortar && !bCombat.IsFiringCanons && bCombat.IsBodySlamReady)
        {
            //agent.ResetPath();
            CharacterStats targetStats = targetTransform.GetComponent<CharacterStats>();

            if (targetStats != null)
            {
                bCombat.BodySlam(targetTransform);
            }

            //FaceTarget(target);
        }

        /*
        if (!bCombat.isAttacking && !bCombat.isFiringMortar && !bCombat.IsBodySlamming && bCombat.IsCanonsReady)
        {
            agent.ResetPath();
            CharacterStats targetStats = target.GetComponent<CharacterStats>();

            if (targetStats != null)
            {
                bCombat.FireCanons();
            }

            //FaceTarget(target);
        }
        */
        



        CheckIfStillInCombat();
    }
}
