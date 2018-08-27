using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Networking;

public class AbilityBodySlam : NetworkBehaviour
{
    [Header("Setup Fields")]
    public GameObject colliderPrefab;
    public GameObject spawnIndicatorPrefab;
    private GameObject spawnIndicatorGO;
    public float slamDelay = 1;
    public float delayToNextAction = 0.5f;
    [Header("Ability Settings")]
    public float range = 1;
    public float chargeSpeed = 6;
    private float defaultSpeed;
    public float damage = 50;

    [HideInInspector]
    public bool isInAbility = false;

    NavMeshAgent agent;
    private Transform target;
    private float distanceToTarget = float.MaxValue;
    private bool isRunningToTarget = false;

    private AbilityCircularCanon canons;
    BossAnimator animator;
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        canons = GetComponent<AbilityCircularCanon>();
        defaultSpeed = agent.speed;
        animator = GetComponent<BossAnimator>();
    }

    private void Update()
    {
        if (!isRunningToTarget) return;

        RunToTarget(target);
        distanceToTarget = Vector3.Distance(target.transform.position, transform.position);
        if (distanceToTarget <= range)
        {
            Slam();
            isRunningToTarget = false;
            agent.speed = defaultSpeed;
        }
    }

    public void Execute(Transform targetTransform)
    {
        isRunningToTarget = true;
        isInAbility = true;
        distanceToTarget = float.MaxValue;
        target = targetTransform;
        animator.StartSprintAnimation();
    }

    private void RunToTarget(Transform targetTransform)
    {
        agent.SetDestination(targetTransform.position);
        agent.speed = chargeSpeed;
        //Debug.Log("run");
    }

    private void Slam()
    {
        //agent.ResetPath();
        animator.StartSlamAnimation();
        agent.SetDestination(transform.position);
        StartCoroutine(SpawnCollider(transform.position, slamDelay));       
    }

    public IEnumerator SpawnCollider(Vector3 spawnPosition, float delay)
    {
        if (isServer)
        {
            CmdSpawnIndicatorOnServer(spawnPosition);
        }
        else
        {
            //TellServerToSpawnIndicator(spawnPosition);
        }

        yield return new WaitForSeconds(delay);
        if (isServer)
        {
            CmdSpawnColliderOnServer(spawnPosition);
            CmdDestroyIndicatorOnServer();
        }
        else
        {
            //TellServerToSpawnCollider(spawnPosition);
            //TellServerToDestroyIndicator();
        }
        yield return new WaitForSeconds(delayToNextAction);
        canons.Execute();
        isInAbility = false;
    }

    [Command]
    void CmdSpawnColliderOnServer(Vector3 spawnPosition)
    {
        GameObject colliderGO = Instantiate(colliderPrefab, spawnPosition, colliderPrefab.transform.rotation);
        colliderGO.GetComponent<BodySlamCollider>().Init(damage);
        //spellScript.Init(this.transform, damage, healAmount);
        NetworkServer.Spawn(colliderGO);
    }
    /*
    [ClientCallback]
    void TellServerToSpawnCollider(Vector3 spawnPosition)
    {
        if (!isServer)
        {
            CmdSpawnColliderOnServer(spawnPosition);
        }
    }
    */

    [Command]
    void CmdSpawnIndicatorOnServer(Vector3 spawnPosition)
    {
        spawnIndicatorGO = Instantiate(spawnIndicatorPrefab, spawnPosition, spawnIndicatorPrefab.transform.rotation);
        NetworkServer.Spawn(spawnIndicatorGO);
    }
    /*
    [ClientCallback]
    void TellServerToSpawnIndicator(Vector3 spawnPosition)
    {
        if (!isServer)
        {
            CmdSpawnIndicatorOnServer(spawnPosition);
        }
    }
    */

    [Command]
    void CmdDestroyIndicatorOnServer()
    {
        NetworkServer.Destroy(spawnIndicatorGO);
    }
    /*
    [ClientCallback]
    void TellServerToDestroyIndicator()
    {
        CmdDestroyIndicatorOnServer();
    }
    */

}
