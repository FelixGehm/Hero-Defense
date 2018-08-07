using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TODO: Rename "InventoryManager"

public class Inventory : MonoBehaviour
{
    #region Singleton
    public static Inventory instance;
    void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one intance of Inventory!");
            return;
        }
        instance = this;
    }
    #endregion

    public CurrenciesUI currenciesUI;

    public delegate void OnItemChanged();
    public OnItemChanged onItemChangedCallback;

    public int space = 20;

    public List<Item> items = new List<Item>();

    //return if item was successfully added
    public bool Add(Item item)
    {
        if (items.Count >= space)
        {
            Debug.Log("Not enough room.");
            return false;
        }
        items.Add(item);

        if (onItemChangedCallback != null)
            onItemChangedCallback.Invoke();

        return true;
    }

    public void Remove(Item item)
    {
        items.Remove(item);
        onItemChangedCallback.Invoke();
    }

    #region Currencies

    private int cash = 0;
    public int Cash
    {
        private set
        {
            cash = value;

            currenciesUI.SetCash(cash);

        }

        get
        {
            return cash;
        }
    }

    public bool SpendCash(int amount)
    {
        if (amount > Cash || amount < 0)
        {
            return false;
        }
        else
        {
            Cash -= amount;
            return true;
        }
    }
    public bool AddCash(int amount)
    {
        if (amount < 0)
        {
            return false;
        }
        else
        {
            Cash += amount;
            return true;
        }
    }


    private int parts = 0;
    public int Parts
    {
        private set
        {
            parts = value;

            currenciesUI.SetParts(parts);
        }

        get
        {
            return parts;
        }
    }

    public bool SpendParts(int amount)
    {
        if (amount > Parts || amount < 0)
        {
            return false;
        }
        else
        {
            Cash -= amount;
            return true;
        }
    }
    public bool AddParts(int amount)
    {
        if (amount < 0)
        {
            return false;
        }
        else
        {
            Parts += amount;
            return true;
        }
    }


    #endregion
}
