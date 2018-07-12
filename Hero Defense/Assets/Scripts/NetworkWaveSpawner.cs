using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkWaveSpawner : NetworkBehaviour
{
    public GameObject enemyMeleePrefab;
    public GameObject enemyRangePrefab;

    // Values for autoSpawning
    [Header("auto spawning")]
    public bool autoSpawn = false;
    public float timeBetweenWavesInSec = 4.0f;

    public float waveCoolDown;

    public int waveCounter = 0;

    // General Values
    [Header("general")]
    public int enemysPerWave = 5;
    public float waveSpawnDuration = 4.0f;

    private void Awake()
    {
        waveCoolDown = timeBetweenWavesInSec;
    }


    void Update()
    {
        if (Input.GetKeyDown("a") && isServer)
        {
            autoSpawn = !autoSpawn;
        }

        if (!autoSpawn)
        {
            if (Input.GetKeyDown("d"))
            {
                Spawn(enemysPerWave, waveSpawnDuration, enemyRangePrefab);
            }
        }

        if (!autoSpawn)
        {
            if (Input.GetKeyDown("s"))
            {
                Spawn(enemysPerWave, waveSpawnDuration, enemyMeleePrefab);
            }
        }
        else
        {
            waveCoolDown -= Time.deltaTime;

            if (waveCoolDown <= 0)
            {
                waveCounter++;
                if (waveCounter % 2 == 0)
                {
                    Spawn(enemysPerWave, waveSpawnDuration, enemyMeleePrefab);
                }
                else
                {
                    Spawn(enemysPerWave, waveSpawnDuration, enemyRangePrefab);
                }

            }
        }
    }

    private void Spawn(int no, float timeSpan, GameObject prefabToSpawn)
    {
        float gap = timeSpan / no;

        for (int i = 0; i < no; i++)
        {
            float delay = i * gap;
            //StartCoroutine("SpawnSingle", delay, prefabToSpawn);
            StartCoroutine(SpawnSingle(delay, prefabToSpawn));
        }
        if (autoSpawn)
        {
            waveCoolDown = timeBetweenWavesInSec + timeSpan;
        }
    }

    IEnumerator SpawnSingle(float delay, GameObject prefabToSpawn)
    {
        yield return new WaitForSeconds(delay);

        Quaternion rot = transform.rotation;
        Vector3 pos = transform.position;

        GameObject enemy = Instantiate(prefabToSpawn, pos, rot);
        NetworkServer.Spawn(enemy);
    }

}
