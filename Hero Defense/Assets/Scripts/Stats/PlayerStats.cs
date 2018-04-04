using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : CharacterStats {

	// Use this for initialization
	void Start () {
        EquipmentManager.instance.onEquipmentChangedCallback += OnEquipmentChanged;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnEquipmentChanged(bool wasAdded, Equipment equipment)
    {
        if (wasAdded)
        {
            //Add Modfifiers
            physicalDamage.AddModifier(equipment.damage);
            armor.AddModifier(equipment.armor);

        }
        else
        {
            //Remove Modfiers
            physicalDamage.RemoveModifier(equipment.damage);
            armor.RemoveModifier(equipment.damage);
        }
    }

    public override void Die()
    {
        base.Die();
        //PlayerManager.instance.KillPlayer();
    }
}
