using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Equipment", menuName = "Items/Equipment")]
public class Equipment : Item
{
    //nur vorschläge
    //die werte werden dann einfach auf die vorhandenen player stats aufaddiert
    public int damage = 0;
    public int critChance = 0;
    public int critDamage = 0;
    public int attackSpeed = 0;

    public int armor;
    //TODO: Elementar Resistenzen, Elementar Schadensarten

    public int moveSpeed = 0;

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
