using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AbilityUI : MonoBehaviour {

    private AbilityBasic ability;
    public Image img;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (ability == null) return;

        img.fillAmount = ability.currentCooldown / ability.abilityCooldown;
	}

    public void RegisterAbilityToUI(AbilityBasic ability)
    {
        this.ability = ability;
    }
}
