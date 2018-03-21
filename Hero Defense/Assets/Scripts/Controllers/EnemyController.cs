﻿using System.Collections;
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
    NetworkCharacterCombat networkCombat;

    bool isNetworkEnemy = false;



    // Use this for initialization
    void Start()
    {
        SetupEnemy();

        destination = PlayerManager.instance.nexus.transform;
        agent = GetComponent<NavMeshAgent>();
        combat = GetComponent<CharacterCombat>();
        if (combat == null)
        {
            isNetworkEnemy = true;
            networkCombat = GetComponent<NetworkCharacterCombat>();
        }
    }

    public void SetupEnemy()
    {
        target = FindClosestPlayer().transform;
    }

    void Update()
    {
        if (target == null && !isTaunted)
        {
            target = FindClosestPlayer().transform;
        }
        else
        {
            distanceToTarget = Vector3.Distance(target.position, transform.position);
        }

        distanceToDestination = Vector3.Distance(destination.position, transform.position);

        //vielleicht lieber über eine coroutine. könnte mit mehreren gegnern etwas viel perfomance schlucken?
        //hier vielleicht ab einer bestimmenten distanz zum nexus den Spieler ignorieren?
        if (distanceToTarget <= lookRadius || isTaunted)
        {
            //Moving to Player and attack
            agent.SetDestination(target.position);

            if (distanceToTarget <= agent.stoppingDistance)
            {
                //CharacterStats targetStats = target.GetComponent<CharacterStats>();
                CharacterStats targetStats = target.GetComponent<CharacterStats>();
                //Debug.Log("targetStats= "+targetStats);
                if (targetStats != null)
                {
                    if (!isNetworkEnemy)
                    {
                        combat.Attack(targetStats);
                    }
                    else
                    {
                        NetworkCharacterStats ncs = target.GetComponent<NetworkCharacterStats>();
                        networkCombat.Attack(ncs);
                    }

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

    #region Taunt

    public bool isTaunted = false;

    /// <summary>
    /// TauntAbility 
    /// </summary>
    /// <param name="tauntTarget"></param>
    /// <param name="duration"></param>
    public void GetTaunted(Transform tauntTarget, float duration)
    {
        Debug.Log("enemy got taunted from " + tauntTarget.name);

        isTaunted = true;

        target = tauntTarget;

        StartCoroutine(EndTauntAfter(duration));
    }

    private IEnumerator EndTauntAfter(float duration)
    {
        yield return new WaitForSeconds(duration);
        isTaunted = false;

    }
    #endregion 


    // Returns the clostest Player to the Enemy
    // If there are no Players registered in the PlayerManager, then returns null
    private GameObject FindClosestPlayer()
    {
        float distanceToPlayer = float.MaxValue;
        GameObject closestPlayer = null;


        for (int i = 0; i < PlayerManager.instance.players.Length; i++)
        {
            if (PlayerManager.instance.players[i] != null)
            {
                float distance = Vector3.Distance(PlayerManager.instance.players[i].transform.position, transform.position);
                if (distanceToPlayer > distance)
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
