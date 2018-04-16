using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
//using UnityEngine.Networking;

public class EnemyController : CrowdControllable
{

    public float lookRadius = 10;


    Transform destination;  //der nexus

    public Transform target;       //der nächstgelegene spieler     // nach tests wieder protected machen!!!


    float distanceToTarget;
    float distanceToDestination;

    NavMeshAgent agent;
    CharacterCombat combat;

    public override void Start()
    {
        base.Start();

        destination = PlayerManager.instance.nexus.transform;
        agent = GetComponent<NavMeshAgent>();
        combat = GetComponent<CharacterCombat>();
    }


    void Update()
    {
        if (myStatuses.Contains(Status.stunned))
        {
            // Tue nichts, solange bis der Stun vorbei ist.
        }
        else
        {
            if (target == null && !myStatuses.Contains(Status.taunted))
            {
                target = FindClosestPlayer().transform;               
            }

            distanceToTarget = Vector3.Distance(target.position, transform.position);
            distanceToDestination = Vector3.Distance(destination.position, transform.position);

            //hier vielleicht ab einer bestimmenten distanz zum nexus den Spieler ignorieren?
            if (distanceToTarget <= lookRadius || myStatuses.Contains(Status.taunted))
            {
                //Moving to Player and attack
                agent.SetDestination(target.position);

                if (distanceToTarget <= agent.stoppingDistance)
                {
                    CharacterStats targetStats = target.GetComponent<CharacterStats>();

                    if (targetStats != null)
                    {
                        Debug.Log("Attack!");
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

    #region CrowdControlable

    public override IEnumerator GetTaunted(Transform tauntTarget, float duration)
    {

        while (myStatuses.Contains(Status.taunted))    // Da man nur von einem Ziel gleichzeitig getaunted sein kann, werden zunächst alle bestehenden Taunts entfernt!
        {
            myStatuses.Remove(Status.taunted);
        }

        myStatuses.Add(Status.taunted);

        target = tauntTarget;

        yield return new WaitForSeconds(duration);
        myStatuses.Remove(Status.taunted);
    }

    public override IEnumerator GetStunned(float duration)
    {
        myStatuses.Add(Status.stunned);
        agent.SetDestination(transform.position);       // agent-Destination auf aktuelle Position setzen

        yield return new WaitForSeconds(duration);
        
            myStatuses.Remove(Status.stunned);
    }

    public override IEnumerator GetSilenced(float duration)
    {
        Debug.Log("Keine Implementierung. Wird erst bei Bossen wichtig!");
        throw new System.NotImplementedException();

    }

    public override IEnumerator GetBlinded(float duration)
    {
        combat.isBlinded = true;
        yield return new WaitForSeconds(duration);
        combat.isBlinded = false;
    }

    public override IEnumerator GetCrippled(float duration, float percent)
    {
        float oldSpeed = agent.speed;

        agent.speed = percent * oldSpeed;

        yield return new WaitForSeconds(duration);

        agent.speed = oldSpeed;
    }

    public override IEnumerator GetBleedingWound(int ticks, float percentPerTick)
    {
        //myStatuses.Add(Status.bleeding);    // Für Variante mit Ticks in jedem Frame
        yield return new WaitForSeconds(0.3f);

        CharacterStats myStats = combat.GetCharacterStats();

        float damageDealed = myStats.CurrentHealth * percentPerTick;
        myStats.TakeTrueDamage(damageDealed);

        ticks -= 1;
        if (ticks > 0)
        {
            StartCoroutine(GetBleedingWound(ticks, percentPerTick));
        }

        //myStatuses.Remove(Status.bleeding);
    }
    #endregion

    private void OnDestroy()
    {
        StopAllCoroutines();
    }
}
