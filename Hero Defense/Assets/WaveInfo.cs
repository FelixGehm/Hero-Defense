using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WaveInfo : MonoBehaviour {

    public Text text;

    public Color textColor;

    public NetworkWaveSpawner spawner;


	// Use this for initialization
	void Start () {

        text.color = textColor;
	}

    int oldCooldown = 0;
    int oldWave = 0;
	
	// Update is called once per frame
	void Update () {
        if ( spawner == null)
        {
            return;
        }

        int cooldown = (int) spawner.waveCoolDown;

        if(cooldown != oldCooldown || oldWave != spawner.waveCounter)
        {
            oldCooldown = cooldown;

            text.text = "Wave " + spawner.waveCounter + "; Cooldown: " + cooldown +"; autospawn = " + spawner.autoSpawn;
        }


        

    }
}
