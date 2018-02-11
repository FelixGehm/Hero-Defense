using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class EnemyController : MonoBehaviour
{

    public float lookRadius = 10;


    Transform destination;  //der nexus
    Transform target;       //der spieler

    float distanceToTarget;
    float distanceToDestination;

    NavMeshAgent agent;
    CharacterCombat combat;

    // Use this for initialization
    void Start()
    {
        target = PlayerManager.instance.player.transform;
        destination = PlayerManager.instance.nexus.transform;
        agent = GetComponent<NavMeshAgent>();
        combat = GetComponent<CharacterCombat>();
    }

    // Update is called once per frame
    void Update()
    {
        distanceToTarget = Vector3.Distance(target.position, transform.position);
        distanceToDestination = Vector3.Distance(destination.position, transform.position);

        //vielleicht lieber über eine coroutine. könnte mit mehreren gegnern etwas viel perfomance schlucken?
        //hier vielleicht ab einer bestimmenten distanz zum nexus den Spieler ignorieren?
        if (distanceToTarget <= lookRadius)
        {
            //Moving to Player and attack
            agent.SetDestination(target.position);

            if (distanceToTarget <= agent.stoppingDistance)
            {
                CharacterStats targetStats = target.GetComponent<CharacterStats>();
                if (targetStats != null)
                {
                    combat.Attack(targetStats);
                }

                FaceTarget(target);
            }

        }
        else
        {
            //Moving to Nexus
            agent.SetDestination(destination.position);
            if (distanceToDestination <= agent.stoppingDistance)
            {
                FaceTarget(destination);
            }
        }
    }

    void FaceTarget(Transform _target)
    {
        Vector3 direction = (_target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, lookRadius);
    }
}
