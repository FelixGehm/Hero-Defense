using UnityEngine;
using System.Collections;
using UnityEngine.AI;


public class JungleMobController : EnemyController
{

    public Transform homeTransform;
    public JungleCamp jungleCamp;

    [HideInInspector]
    public float followRange = 0.0f;        // gets set from JungleCamp during spawn

    private EnemyStats myStats;

    public float maxDistanceToHome;



    public override void Awake()
    {
        base.Awake();

        //destination = PlayerManager.instance.nexus.transform;
        agent = GetComponent<NavMeshAgent>();
        combat = GetComponent<CharacterCombat>();

        myStats = GetComponent<EnemyStats>();
    }

    private void Start()
    {
        //nexus = PlayerManager.instance.nexus.transform;  //TODO: über GameManager holen

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
            distanceToTarget = Vector3.Distance(targetTransform.position, transform.position);
        }

        if (targetTransform.Equals(homeTransform))
        {
            agent.stoppingDistance = 0.1f;
        }
        else
        {
            agent.stoppingDistance = stoppingDistancePlayer;
        }

        //Moving to target
        if (!combat.isAttacking)
        {
            agent.SetDestination(targetTransform.position);
        }


        if (distanceToTarget <= agent.stoppingDistance + 0.01) //??
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




    //bool isInCombat = false;

    /// <summary>
    /// 
    /// </summary>
    /// <returns> the traget </returns>
    protected override Transform GetTarget()
    {
        float distanceToHome = Vector3.Distance(jungleCamp.transform.position, transform.position);

        // Wenn im fight, behalte das alte Ziel
        if (isInCombat && targetTransform.GetComponent<CharacterStats>().IsAlive() && distanceToHome <= maxDistanceToHome)
        {
            return targetTransform;
        }

        //isInCombat = false;
        return FindTarget();
    }

    private Transform FindTarget()
    {
        Transform target = homeTransform;

        float distanceToPlayer = float.MaxValue;

        float distanceToHome = Vector3.Distance(jungleCamp.transform.position, transform.position);


        if (distanceToHome <= maxDistanceToHome)
        {
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
                        float playerDistanceToHome = Vector3.Distance(player.transform.position, homeTransform.position);
                        if (distanceToPlayer > distance && distance <= lookRadius && playerDistanceToHome <= maxDistanceToHome)
                        {
                            distanceToPlayer = distance;
                            target = player.transform;
                        }
                    }
                }
            }
        }
        return target;
    }


    protected override void CheckIfStillInCombat()
    {
        distanceToTarget = Vector3.Distance(targetTransform.position, transform.position);
        float distanceToHome = Vector3.Distance(jungleCamp.transform.position, transform.position);


        if (distanceToHome > maxDistanceToHome || (targetTransform.GetComponent<CharacterStats>() != null && !targetTransform.GetComponent<CharacterStats>().IsAlive()))
        {
            myStats.TakeHeal(float.MaxValue);
            isInCombat = false;
        }
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

        jungleCamp.Respawn();
    }
}
