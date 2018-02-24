using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
//using UnityEngine.Networking;

public class EnemyController : MonoBehaviour
{

    public float lookRadius = 10;


    Transform destination;  //der nexus
    Transform target;       //der nächstgelegene spieler

    float distanceToTarget;
    float distanceToDestination;

    NavMeshAgent agent;
    CharacterCombat combat;

    // Use this for initialization
    void Start()
    {
        SetupEnemy();

        destination = PlayerManager.instance.nexus.transform;
        agent = GetComponent<NavMeshAgent>();
        combat = GetComponent<CharacterCombat>();
    }

    public void SetupEnemy()
    {
        target = FindClosestPlayer().transform;
        
    }

    void Update()
    {
        if ( target == null)
        {
            target = FindClosestPlayer().transform;
        } else
        {
            distanceToTarget = Vector3.Distance(target.position, transform.position);
        }
        
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



    // Returns the clostest Player to the Enemy
    // If there are no Players registered in the PlayerManager, then returns null
    private GameObject FindClosestPlayer()
    {
        float distanceToPlayer = float.MaxValue;
        GameObject closestPlayer = null;


        for (int i = 0; i< PlayerManager.instance.players.Length; i++)
        {
            if (PlayerManager.instance.players[i] != null)
            {
                float distance = Vector3.Distance(PlayerManager.instance.players[i].transform.position, transform.position);
                if (distanceToPlayer > distance )
                {
                    distanceToPlayer = distance;
                    closestPlayer = PlayerManager.instance.players[i];
                }
            }                        
        }
        return closestPlayer;
        
    }


    // Draw LookRadius in Editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, lookRadius);
    }
}
