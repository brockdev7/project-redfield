using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour, ISelectHandler, IDeselectHandler, IMoveHandler , ISubmitHandler
{
    [SerializeField] public int id;
    [SerializeField] public bool isSelected = false;
    [SerializeField] public Image slotImage;
    [SerializeField] public Image icon;
    [SerializeField] public string itemName;
    [SerializeField] public string itemDesc;

    public GameObject renderModel;
    
    public static event Action<InventorySlot> OnInventorySlotSubmit;
    public static event Action<InventorySlot> OnInventorySlotMove;

    private Color defaultColor = new Color(255, 255, 255, 0.1294118f);
    private Color selectedColor = new Color(255, 255, 255, 1f);
    public void SetDefaultColor() => slotImage.color = defaultColor;
    public void SetSelectedColor() => slotImage.color = selectedColor;

    //Initialize Slot
    public void Set(ItemData itemData)
    {
        icon.sprite = itemData.icon;
        icon.enabled = true;
        itemName = itemData.itemName;
        itemDesc = itemData.itemDescription; 
        renderModel = itemData.renderModel;
    }

    //Remove Slot
    public void Remove()
    {
        icon = null;
        icon.enabled = false;
        itemName = String.Empty;
        itemDesc = String.Empty;
        renderModel = null;
    }

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
        slotImage.color = selectedColor;
    }

    public void OnDeselect(BaseEventData eventData)
    {
        slotImage.color = defaultColor;
    }

    public void OnSubmit(BaseEventData eventData)
    {
        OnInventorySlotSubmit?.Invoke(this);
    }
}
