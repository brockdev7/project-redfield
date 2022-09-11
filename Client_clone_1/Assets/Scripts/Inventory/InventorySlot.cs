using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour, ISelectHandler, IDeselectHandler, IMoveHandler , ISubmitHandler
{
    [SerializeField] public bool isSelected = false;
    [SerializeField] public bool hasItem = false;
    [SerializeField] public Image icon;

    public static event Action<InventorySlot> OnInventorySlotSubmit;
    public static event Action<InventorySlot> OnInventorySlotMove;

    private Color defaultColor = new Color(255, 255, 255, 0.1294118f);
    private Color selectedColor = new Color(255, 255, 255, 1f);

    private ItemData itemData;
    public ItemData ItemData => itemData;

    public void SetItemData(ItemData _item)
    {
        itemData = _item;
    }

    public void SetDefaultColor() => icon.color = defaultColor;
    public void SetSelectedColor() => icon.color = selectedColor;

    public void OnMove(AxisEventData eventData)
    {
        isSelected = eventData.selectedObject == this.gameObject ? true : false;

        var invSlot = eventData.selectedObject.GetComponent<InventorySlot>();

        if (invSlot)
            OnInventorySlotMove?.Invoke(invSlot);          
    }

    public void OnSelect(BaseEventData eventData)
    {
        isSelected = eventData.selectedObject == this.gameObject ? true : false;
        icon.color = selectedColor;
    }

    public void OnDeselect(BaseEventData eventData)
    {
        icon.color = defaultColor;
    }

    //Item Added to Slot
    //Submit buttons changed to either (Return) or (Space)
    public void OnSubmit(BaseEventData eventData)
    {
        OnInventorySlotSubmit?.Invoke(this);
    }
}
