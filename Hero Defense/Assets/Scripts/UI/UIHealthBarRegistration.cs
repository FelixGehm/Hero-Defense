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

        if(GameObject.Find("UIHealthBar"))
        {
            uiHealthBar = GameObject.Find("UIHealthBar").GetComponent<UIHealthBar>();

            if (isLocalPlayer)
            {
                uiHealthBar.RegisterPlayerStats(stats);
            }
        } else
        {
            Debug.LogWarning("Couldn't find UIHealthBar Script");
        }
	}
	

}
