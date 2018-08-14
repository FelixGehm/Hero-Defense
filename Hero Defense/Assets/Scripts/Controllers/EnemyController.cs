using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
//using UnityEngine.Networking;

public class EnemyController : CrowdControllable
{

    public float lookRadius = 10;


    /*
     * Movement Behaviour
     */
    public float waitProbability = 0.04f;

    public float minimumTimeBetweenRandomStops = 2.5f;
    private float timeSinceLastStop = 0;

    public float waitDurationMin = 0.5f;
    public float waitDurationMax = 1.3f;

    public float bufferDistanceToTarget = 1.4f;

    protected Transform nexus;  //der nexus

    public Waypoint currentWaypoint;
    public Vector3 currentWaypointDestination;
    

    public Transform targetTransform;       // das Ziel des Gegners


    protected float distanceToTarget = float.MaxValue;

    protected NavMeshAgent agent;
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


    protected virtual void Update()
    {
        /*
         * Movement Behaviour
         */
        timeSinceLastStop += Time.deltaTime;

        if(isWaiting)
        {
            if(CheckIfWaitingShouldEnd())
            {
                isWaiting = false;
                agent.isStopped = false;
            } 
            else
            {
                return;
            }
        }
        else if(timeSinceLastStop >= minimumTimeBetweenRandomStops && targetTransform.GetComponent<Waypoint>() != null)
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
            distanceToTarget = Vector3.Distance(targetTransform.position, transform.position);
        }

        if (targetTransform.Equals(nexus))
        {
            agent.stoppingDistance = stoppingDistanceNexus;
        }
        else
        {
            agent.stoppingDistance = stoppingDistancePlayer;
        }

        //Moving to Player and attack
        if (!combat.isAttacking)
        {
            if(targetTransform.gameObject.name.Contains("Waypoint"))
            {                
                agent.SetDestination(currentWaypointDestination);
            }
            else
            {
                agent.SetDestination(targetTransform.position);
            }
        }
            

        if (distanceToTarget <= agent.stoppingDistance)
        {
            CharacterStats targetStats = targetTransform.GetComponent<CharacterStats>();

            if (targetStats != null)
            {
                combat.Attack(targetStats);
            }

            FaceTarget(targetTransform);
        }

        CheckIfStillInCombat();
    }

    protected void FaceTarget(Transform _target)
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
    protected Transform GetTarget()
    {
        // Wenn im fight, behalte das alte Ziel
        if (isInCombat && targetTransform.GetComponent<CharacterStats>().IsAlive())
        {
            return targetTransform;
        }

        //isInCombat = false;
        return FindTarget();
    }

    private Transform FindTarget()
    {
        Transform target;
        if (currentWaypoint == null)
        {
            target = nexus;
        }
        else
        {
            target = currentWaypoint.transform;
        }

        float distanceToPlayer = float.MaxValue;

        for (int i = 0; i < PlayerManager.instance.players.Length; i++)     // Liste der Spieler durchgehen
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

    #region Movement Behaviour

    bool isWaiting = false;

    private float timeLeftToWait;

    /// <summary>
    /// Außerhalb des Kampfes wird abhängig von den Paramtertn  zufällig entschieden,
    /// ob der Gegner kurzzeitig stehen bleibt oder nicht.
    /// </summary>
    /// <param name="probability"></param>
    /// <param name="duration"></param>
    private void StayStill(float probability, float duration)
    {
        float r = Random.Range(0, 1.0f);

        if(!isInCombat && r <= probability)
        {            
            isWaiting = true;
            agent.isStopped = true;

            timeLeftToWait = duration;
            timeSinceLastStop = -duration;
        }        
    }

    private bool CheckIfWaitingShouldEnd()
    {
        bool b = false;
        timeLeftToWait -= Time.deltaTime;       

        if(timeLeftToWait <= 0 )
        {
            b = true;
        }

        return b;
    }

    private bool CheckIfWaypointReached()
    {        
        return  Vector3.Distance(transform.position, currentWaypointDestination) <= bufferDistanceToTarget;
    }



    #endregion


    public void ReceivedDamageFrom(Transform damageDealer)
    {
        if (!isInCombat)
        {
            isInCombat = true;
            targetTransform = damageDealer;
        }
    }

    protected void CheckIfStillInCombat()
    {
        distanceToTarget = Vector3.Distance(targetTransform.position, transform.position);
        if (distanceToTarget > lookRadius || (targetTransform.GetComponent<CharacterStats>() != null && !targetTransform.GetComponent<CharacterStats>().IsAlive()))
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

        targetTransform = tauntTarget;

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
