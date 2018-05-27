using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class WaveInfo : NetworkBehaviour {

    public Text text;

    public Color textColor;

    public NetworkWaveSpawner spawner;

    [SyncVar(hook = "OnChangeText")]
    private string networkString;


    void OnChangeText( string newText)
    {
        text.text = newText;
    }

    // Use this for initialization
    void Start () {

        text.color = textColor;
	}

    int oldCooldown = 0;
    int oldWave = 0;
	
	// Update is called once per frame
	void Update () {
        if ( spawner == null || !isServer)
        {
            return;
        }

        int cooldown = (int) spawner.waveCoolDown;

        if(  cooldown != oldCooldown || oldWave != spawner.waveCounter)
        {
            oldCooldown = cooldown;

            networkString = "Wave " + spawner.waveCounter + "; Cooldown: " + cooldown + "; autospawn = " + spawner.autoSpawn;

            text.text = networkString;
        }


        

    }
}
