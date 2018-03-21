using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


public class UIHealthBarRegistration : NetworkBehaviour {


    CharacterStats stats;

    UIHealthBar uiHealthBar;

	// Use this for initialization
	void Start () {

        stats = GetComponent<CharacterStats>();

        uiHealthBar = GameObject.Find("Canvas HUD").transform.Find("CharacterInfo").Find("UIHealthBar").GetComponent<UIHealthBar>();

        if (isLocalPlayer)
        {
            uiHealthBar.RegisterPlayerStats(stats);
        }        
	}
	

}
