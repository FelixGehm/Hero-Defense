﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : CharacterStats {

	// Use this for initialization
	void Start () {
        EquipmentManager.instance.onEquipmentChangedCallback += OnEquipmentChanged;
	}	

    void OnEquipmentChanged(bool wasAdded, Equipment equipment)
    {
        if (wasAdded)
        {
            //Add Modfifiers
            physicalDamage.AddModifier(equipment.damage);
            armor.AddModifier(equipment.armor);
            attackSpeed.AddModifier(equipment.attackSpeed);
        }
        else
        {
            //Remove Modfiers
            physicalDamage.RemoveModifier(equipment.damage);
            armor.RemoveModifier(equipment.damage);
            attackSpeed.RemoveModifier(equipment.attackSpeed);
        }
    }

    public override void Die()
    {
        base.Die();
        //PlayerManager.instance.KillPlayer();
    }
}
