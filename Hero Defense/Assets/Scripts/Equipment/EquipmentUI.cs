using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentUI : MonoBehaviour
{

    public Transform slotsParent;

    EquipmentManager equipmentManager;

    EquipmentSlot[] slots;

    // Use this for initialization
    void Start()
    {
        equipmentManager = EquipmentManager.instance;
        equipmentManager.onEquipmentChangedCallback += UpdateUI;

        slots = slotsParent.GetComponentsInChildren<EquipmentSlot>();
    }

    void UpdateUI(bool wasAdded, Equipment equip)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (i < equipmentManager.equipments.Count)
            {
                slots[i].AddEquipment(equipmentManager.equipments[i]);
            }
            else
            {
                slots[i].ClearSlot();
            }
        }
    }
}
