using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentManager : MonoBehaviour
{

    #region Singleton
    public static EquipmentManager instance;
    void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one intance of EquipmentManager!");
            return;
        }
        instance = this;
    }
    #endregion

    public int space = 6;

    public List<Equipment> equipments = new List<Equipment>();

    Inventory inventory;

    //Callback:
    public delegate void OnEquipmentChanged(bool wasAdded, Equipment equip);
    public OnEquipmentChanged onEquipmentChangedCallback;

    void Start()
    {
        inventory = Inventory.instance;
    }

    //return if item was successfully added
    public bool Equip(Equipment equip)
    {
        if (equipments.Count >= space)
        {
            Debug.Log("Not enough room.");
            return false;
        }
        equipments.Add(equip);

        if (onEquipmentChangedCallback != null)
            onEquipmentChangedCallback.Invoke(true, equip);


        return true;
    }

    //puts item back into inventory
    public void UnEquip(Equipment equip)
    {
        if (inventory.Add(equip))
        {
            equipments.Remove(equip);

            if (onEquipmentChangedCallback != null)
                onEquipmentChangedCallback.Invoke(false, equip);
        }
        else
        {
            Debug.Log("Not enough Space in Inventory");
        }

    }
}

