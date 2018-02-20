using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveSpawner : MonoBehaviour {

    public GameObject toSpawnPrefab;

    public float timeBetweenWavesInSec = 4.0f;
    
    // Use this for initialization
	void Start () {
		
	}
	
	
    
    // Update is called once per frame
	void Update () {
        if (Input.GetKeyDown("s"))
        {
            Spawn(5, 3.0f);
        }
	}

    private void Spawn(int no, float timeSpan)
    {
        float gap = timeSpan / no;

        for (int i = 0; i <= no; i++)
        {
            float delay = i * gap;
            StartCoroutine("SpawnSingle", delay);
        }
    }

    IEnumerator SpawnSingle(float delay)
    {
        yield return new WaitForSeconds(delay);
        Instantiate(toSpawnPrefab,this.transform);
    }

}
