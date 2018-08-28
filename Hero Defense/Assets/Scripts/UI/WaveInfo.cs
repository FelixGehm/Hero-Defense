using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class WaveInfo : NetworkBehaviour {

    public Text text;

    public Color textColor;
    
    public WaveManager waveManager;



    [SyncVar(hook = "OnChangeText")]
    private string networkString;


    void OnChangeText( string newText)
    {
        text.text = newText;
    }
        
    void Start () {
        text.color = textColor;
	}

    int oldCooldown = 0;
    int oldWave = 0;
	
	// Update is called once per frame
	void Update () {
        if ( waveManager == null || !isServer)
        {
            return;
        }

        int cooldown = (int) waveManager.TimeTillNextWave;

        if(  cooldown != oldCooldown || oldWave != waveManager.currentWaveNo)
        {
            oldCooldown = cooldown;

            networkString = "Wave " + waveManager.currentWaveNo + " in " + cooldown + " Seconds";

            text.text = networkString;
        }
    }
}
