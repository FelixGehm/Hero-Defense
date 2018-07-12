using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
//using UnityEngine.Networking;

public class EnemyController : CrowdControllable
{

    public float lookRadius = 10;


    Transform nexus;  //der nexus
    
    public Transform target;       // das Ziel des Gegners


    float distanceToTarget = float.MaxValue;

    NavMeshAgent agent;
    protected CharacterCombat combat;

    //Update the stopping Distances based on the target's size
    [Header("Agent Stopping Distances - Player")]
    public float stoppingDistancePlayer = 1.2f;

    [Header("Agent Stopping Distances - Nexus")]
    public float stoppingDistanceNexus = 2;

    public override void Awake()
    {
        base.Awake();

        //destination = PlayerManager.instance.nexus.transform;
        agent = GetComponent<NavMeshAgent>();
        combat = GetComponent<CharacterCombat>();
    }

    private void Start()
    {
        //nexus = PlayerManager.instance.nexus.transform;  //TODO: über GameManager holen
        nexus = GameManager.instance.nexus.transform;
    }


    void Update()
    {
        if (myStatuses.Contains(Status.stunned))
        {
            // Tue nichts, solange bis der Stun vorbei ist.
            return;
        }

        if (!myStatuses.Contains(Status.taunted) )
        {
            target = GetTarget();
        }

        if (target.Equals(nexus))
        {
            agent.stoppingDistance = stoppingDistanceNexus;
        }
        else
        {
            agent.stoppingDistance = stoppingDistancePlayer;
        }

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

        CheckIfStillInCombat();
    }

    void FaceTarget(Transform _target)
    {
        Vector3 direction = (_target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5);
    }


    bool isInCombat = false;

    /// <summary>
    /// 
    /// </summary>
    /// <returns> the traget </returns>
    private Transform GetTarget()
    {
        // Wenn im fight, behalte das alte Ziel
        if (isInCombat && target.GetComponent<CharacterStats>().IsAlive())
        {
            return target;            
        }

        //isInCombat = false;
        return FindTarget();
    }

    private Transform FindTarget()
    {
        Transform target = nexus;

        float distanceToPlayer = float.MaxValue;               

        for (int i = 0; i < PlayerManager.instance.players.Length; i++)     // Liste der Spielrr durchgehen
        {
            if (PlayerManager.instance.players[i] != null)
            {
                GameObject player = PlayerManager.instance.players[i];

                bool playerIsAlive = player.GetComponent<PlayerStats>().IsAlive();

                // Wenn Spieler am Leben, Entfernung überprüfen und ggf. als Target setzen
                if (playerIsAlive)  
                {
                    float distance = Vector3.Distance(player.transform.position, transform.position);
                    if (distanceToPlayer > distance && distance <= lookRadius)
                    {
                        distanceToPlayer = distance;
                        target = player.transform;
                    }
                }
            }
        }
        return target;
    }


    public void ReceivedDamageFrom(Transform damageDealer)
    {
        if(!isInCombat)
        {
            isInCombat = true;
            target = damageDealer;
        }        
    }

    private void CheckIfStillInCombat()
    {
        distanceToTarget = Vector3.Distance(target.position, transform.position);
        if (distanceToTarget > lookRadius || !target.GetComponent<CharacterStats>().IsAlive())
        {
            isInCombat = false;
        }
        
    }

    // Draw LookRadius in Editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, lookRadius);
    }

    #region CrowdControlable

    protected override IEnumerator GetTauntedCo(Transform tauntTarget, float duration)
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

    protected override IEnumerator GetStunnedCo(float duration)
    {
        //Debug.Log("Stun with duration = " +duration);

        while (myStatuses.Contains(Status.stunned))
        {
            myStatuses.Remove(Status.stunned);
        }


        myStatuses.Add(Status.stunned);
        agent.SetDestination(transform.position);       // agent-Destination auf aktuelle Position setzen

        yield return new WaitForSeconds(duration);

        myStatuses.Remove(Status.stunned);
    }

    protected override IEnumerator GetSilencedCo(float duration)
    {
        Debug.Log("Keine Implementierung. Wird erst bei Bossen wichtig!");
        throw new System.NotImplementedException();

    }

    protected override IEnumerator GetBlindedCo(float duration)
    {
        combat.isBlinded = true;
        yield return new WaitForSeconds(duration);
        combat.isBlinded = false;
    }

    protected override IEnumerator GetCrippledCo(float duration, float percent)
    {
        float oldSpeed = agent.speed;

        agent.speed = percent * oldSpeed;

        yield return new WaitForSeconds(duration);

        agent.speed = oldSpeed;
    }

    protected override IEnumerator GetBleedingWoundCo(int ticks, float percentPerTick)
    {
        myStatuses.Add(Status.bleeding);    // Für Variante mit Ticks in jedem Frame
        yield return new WaitForSeconds(0.3f);

        CharacterStats myStats = combat.GetCharacterStats();

        float damageDealed = myStats.SyncedCurrentHealth * percentPerTick;
        myStats.TakeTrueDamage(damageDealed);

        ticks -= 1;
        if (ticks > 0)
        {
            StartCoroutine(GetBleedingWoundCo(ticks, percentPerTick));
        }

        myStatuses.Remove(Status.bleeding);
    }
    #endregion

    private void OnDestroy()
    {
        StopAllCoroutines();
    }
}
