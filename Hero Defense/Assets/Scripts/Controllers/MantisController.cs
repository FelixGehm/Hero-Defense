
// only uses a different Combat Component than the EnemyController
using UnityEngine;

public class MantisController : EnemyController
{
    private float attackRange;
    public override void Awake()
    {
        base.Awake();
        combat = GetComponent<MantisCombat>();
        agent.stoppingDistance = 0;
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
        else if (timeSinceLastStop >= minimumTimeBetweenRandomStops && targetTransform.GetComponent<Waypoint>() != null)
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
        if (!combat.isAttacking)
        {
            if (targetTransform.gameObject.name.Contains("Waypoint"))
            {
                agent.SetDestination(currentWaypointDestination);
            }
            else
            {
                agent.SetDestination(targetTransform.position);
            }
        }

        if (distanceToTarget <= attackRange && targetTransform.GetComponent<CharacterStats>() != null)
        {
            agent.ResetPath();
            //Debug.Log("attack!!!!!!!!!!!!!!!!!!!!!!!!!!");
            CharacterStats targetStats = targetTransform.GetComponent<CharacterStats>();

            if (targetStats != null)
            {
                combat.Attack(targetStats);
            }

            FaceTarget(targetTransform);
        }

        CheckIfStillInCombat();
    }
}
