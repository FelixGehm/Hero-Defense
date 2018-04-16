using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestTauntOnPlayer : MonoBehaviour {


    Transform player;
    MyPlayerController pc;
    // Use this for initialization
    void Start () {
        
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.T))
        {
            player = GetComponent<EnemyController>().target;

            pc = player.GetComponent<MyPlayerController>();


            Debug.Log("TAUNTTEST");
            pc.GetTaunted(transform, 10);
        }

    }
}
