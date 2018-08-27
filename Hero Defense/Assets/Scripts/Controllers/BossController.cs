


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
        /*
         * Movement Behaviour
         */
        timeSinceLastStop += Time.deltaTime;

        if (isWaiting)
        {
            if (CheckIfWaitingShouldEnd())
            {
                isWaiting = false;
                agent.isStopped = false;
            }
            else
            {
                return;
            }
        }
        else if (timeSinceLastStop >= minimumTimeBetweenRandomStops && targetTransform != null && targetTransform.GetComponent<Waypoint>() != null)
        {
            StayStill(waitProbability, Random.Range(waitDurationMin, waitDurationMax));
        }


        if (!currentWaypoint.isNexus && CheckIfWaypointReached())
        {
            currentWaypoint = currentWaypoint.next;
            currentWaypointDestination = currentWaypoint.GetDestinationInRadius();
        }

        /*
        * Movement Behaviour end
        */

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
        {
            if (targetTransform.gameObject.name.Contains("Waypoint"))
            {
                agent.SetDestination(currentWaypointDestination);
            }
            else
            {
                agent.SetDestination(targetTransform.position);
                distanceToTarget = Vector3.Distance(targetTransform.position, transform.position);      //schneller fix: sonst stimmt die ditanceToTarget beim wechseln für einen frame nicht, wodurch fälscherweise eine attack ausgelöst wird
            }
        }




        if (distanceToTarget <= mortarRange && !bCombat.isAttacking && !bCombat.IsBodySlamming && !bCombat.IsFiringCanons && bCombat.IsMortarReady && targetTransform.GetComponent<CharacterStats>() != null)
        {
            agent.ResetPath();
            CharacterStats targetStats = targetTransform.GetComponent<CharacterStats>();

            if (targetStats != null)
            {
                bCombat.MortarAttack(targetTransform.position);
            }

            FaceTarget(targetTransform);
        }

        if (distanceToTarget <= bodySlamRange && !bCombat.isAttacking && !bCombat.isFiringMortar && !bCombat.IsFiringCanons && bCombat.IsBodySlamReady && targetTransform.GetComponent<CharacterStats>() != null)
        {
            //agent.ResetPath();
            CharacterStats targetStats = targetTransform.GetComponent<CharacterStats>();

            if (targetStats != null)
            {
                bCombat.BodySlam(targetTransform);
            }

            //FaceTarget(target);
        }

        if (distanceToTarget <= attackRange && targetTransform.GetComponent<CharacterStats>() != null)
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
