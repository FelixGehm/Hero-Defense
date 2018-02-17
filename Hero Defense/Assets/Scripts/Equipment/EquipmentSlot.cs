using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;

public class EquipmentSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    public Image icon;

    Equipment equipment;

    bool isMouseOver = false;

    void Update()
    {
        if (isMouseOver)
        {
            if (Input.GetMouseButtonDown(1) && equipment != null)
            {
                EquipmentManager.instance.UnEquip(equipment);
            }
        }
    }

    public void AddEquipment(Equipment newItem)
    {
        equipment = newItem;

        icon.sprite = equipment.icon;
        icon.enabled = true;
    }

    public void ClearSlot()
    {
        equipment = null;

        icon.sprite = null;
        icon.enabled = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isMouseOver = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isMouseOver = false;
    }
}
