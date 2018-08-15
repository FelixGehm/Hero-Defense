using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class WaveSpawnerSlave : NetworkBehaviour
{
    public GameObject enemyMeleePrefab;
    public GameObject enemyRangePrefab;
    public GameObject enemySpecialRangePrefab;
    public GameObject enemyBoss;

    public enum Lane { Top, Middle, Bot };

    public Lane lane;

    [HideInInspector]
    public GameObject nexus;

    private void Update()
    {
        if (nexus == null)
        {
            nexus = GameManager.instance.nexus;
        }

    }
       

    public void Spawn(int noOfMelee, int noOfRanged, int noOfSpecialRanged, int noOfBoss, float timeSpan)
    {
        int sum = noOfMelee + noOfRanged + noOfSpecialRanged + noOfBoss;

        float gap = timeSpan / sum;
        int n = 0;

        for (int i = 0; i < noOfMelee; i++)
        {
            float delay = n * gap;
            n++;
            StartCoroutine(SpawnSingle(delay, enemyMeleePrefab));
        }

        for (int i = 0; i < noOfRanged; i++)
        {
            float delay = n * gap;
            n++;
            StartCoroutine(SpawnSingle(delay, enemyRangePrefab));
        }

        for (int i = 0; i < noOfSpecialRanged; i++)
        {
            float delay = n * gap;
            n++;
            StartCoroutine(SpawnSingle(delay, enemySpecialRangePrefab));
        }

        for (int i = 0; i < noOfBoss; i++)
        {
            float delay = n * gap;
            n++;
            StartCoroutine(SpawnSingle(delay, enemyBoss));
        }
    }

    IEnumerator SpawnSingle(float delay, GameObject prefabToSpawn)
    {
        yield return new WaitForSeconds(delay);

        Quaternion rot = transform.rotation;
        Vector3 pos = transform.position;

        GameObject enemy = Instantiate(prefabToSpawn, pos, rot);

        enemy.GetComponent<EnemyController>().currentWaypoint = SetupFirstWaypoint();
        enemy.GetComponent<EnemyController>().currentWaypointDestination = enemy.GetComponent<EnemyController>().currentWaypoint.GetDestinationInRadius();
        NetworkServer.Spawn(enemy);
    }

    private Waypoint SetupFirstWaypoint()
    {
        Waypoint waypoint;

        switch (lane)
        {
            case Lane.Top:

                waypoint = nexus.transform.Find("WayPointsTop").GetChild(0).GetComponent<Waypoint>();
                break;
            case Lane.Middle:

                waypoint = nexus.transform.Find("WayPointsMiddle").GetChild(0).GetComponent<Waypoint>();
                break;
            case Lane.Bot:
                waypoint = nexus.transform.Find("WayPointsBottom").GetChild(0).GetComponent<Waypoint>();
                break;
            default:
                waypoint = nexus.GetComponent<Waypoint>();
                break;
        }
        return waypoint;
    }
}

