using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Equipment", menuName = "Items/Equipment")]
public class Equipment : Item
{
    //nur vorschläge
    //die werte werden dann einfach auf die vorhandenen player stats aufaddiert
    public float damage = 0;
    public float critChance = 0;
    public float critDamage = 0;
    public float attackSpeed = 0;

    public float armor;
    //TODO: Elementar Resistenzen, Elementar Schadensarten

    public float moveSpeed = 0;

    public override void Use()
    {
        base.Use();

        if (EquipmentManager.instance.Equip(this))
            RemoveFromInventory();
    }

    public void RemoveFromEquipmentManager()
    {
        EquipmentManager.instance.UnEquip(this);
    }
}
