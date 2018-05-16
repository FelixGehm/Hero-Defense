using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : CharacterStats
{
    PlayerController pc;
    public override void Awake()
    {
        base.Awake();

        if (GameObject.Find("UIHealthBar"))
        {
            uIHealthBar = GameObject.Find("UIHealthBar").GetComponent<UIHealthBar>();
        }
        else
        {
            Debug.LogWarning("No UIHealthBar Script Found");
        }
    }

    // Use this for initialization
    void Start()
    {
        pc = GetComponent<PlayerController>();
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
        if (isLocalPlayer && pc != null)
        {
            pc.KillPlayer();
            //StartCoroutine(ReviveTest());
        }
    }

    IEnumerator ReviveTest()        //TODO
    {
        yield return new WaitForSeconds(5);

        pc.RevivePlayer();

    }
}
