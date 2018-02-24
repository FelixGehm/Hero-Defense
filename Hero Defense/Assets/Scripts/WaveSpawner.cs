using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class WaveSpawner : NetworkBehaviour {

    public GameObject toSpawnPrefab;

    public float timeBetweenWavesInSec = 4.0f;
    private float waveCoolDown;

    

    public bool autoSpawn = false;

    public int enemysPerWave = 5;
    public float waveSpawnDuration = 4.0f;

    private void Awake()
    {
        waveCoolDown = timeBetweenWavesInSec;
    }

    void FixedUpdate () {
        if(!autoSpawn)
        {
            if (Input.GetKeyDown("s"))
            {
                Spawn(enemysPerWave, waveSpawnDuration);
            }
        }
        else
        {
            waveCoolDown -= Time.deltaTime;

            if (waveCoolDown <= 0)
            {
                Spawn(enemysPerWave, waveSpawnDuration);
            }
        }

        
	}

    private void Spawn(int no, float timeSpan)
    {
        float gap = timeSpan / no;

        for (int i = 0; i < no; i++)
        {
            float delay = i * gap;
            StartCoroutine("SpawnSingle", delay);
        }
        if(autoSpawn)
        {
            waveCoolDown = timeBetweenWavesInSec + timeSpan ;
        }
    }

    IEnumerator SpawnSingle(float delay)
    {
        yield return new WaitForSeconds(delay);
        GameObject enemy =  Instantiate(toSpawnPrefab,this.transform);
        NetworkServer.Spawn(enemy);
    }

}
