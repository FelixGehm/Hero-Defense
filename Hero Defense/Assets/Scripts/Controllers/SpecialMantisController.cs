




// used ranged auto attacks as well as the mortar ability
using UnityEngine;

public class SpecialMantisController : EnemyController
{
    private float attackRange;
    public float mortarRange;
    private MantisCombat mCombat;
    public override void Awake()
    {
        base.Awake();
        mCombat = GetComponent<MantisCombat>();
        attackRange = GetComponent<EnemyStats>().attackRange.GetValue();
        agent.stoppingDistance = 0;
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


        if (currentWaypoint != null && !currentWaypoint.isNexus && CheckIfWaypointReached())
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
        if (!combat.isAttacking && !mCombat.isFiringMortar)
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

        if (distanceToTarget <= attackRange && targetTransform.GetComponent<CharacterStats>() != null)
        {
            agent.ResetPath();
            if (!mCombat.isFiringMortar)
            {
                CharacterStats targetStats = targetTransform.GetComponent<CharacterStats>();

                if (targetStats != null)
                {
                    mCombat.Attack(targetStats);
                    //combat.MortarAttack(target.position);
                }

                FaceTarget(targetTransform);
            }

        }

        if (distanceToTarget <= mortarRange && !mCombat.isAttacking && mCombat.IsMortarReady && targetTransform.GetComponent<PlayerStats>() != null)
        {
            agent.ResetPath();
            CharacterStats targetStats = targetTransform.GetComponent<CharacterStats>();

            if (targetStats != null)
            {
                mCombat.DoubleMortarAttack(targetTransform.position);
            }

            FaceTarget(targetTransform);
        }

        CheckIfStillInCombat();
    }
}
