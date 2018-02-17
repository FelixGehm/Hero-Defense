using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Items/Item")]
public class Item : ScriptableObject {

    //new, weil name bereits vorhanden
    new public string name = "New Item";
    public Sprite icon = null;


    //Unterschiedliche implementierung für z.B. pots oder equipment
    public virtual void Use()
    {
        Debug.Log("Using " + name);
    }

    public void RemoveFromInventory()
    {
        Inventory.instance.Remove(this);
    }


}
