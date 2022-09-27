using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour, ISelectHandler, IDeselectHandler, IMoveHandler , ISubmitHandler
{
    [SerializeField] public ushort id;
    [SerializeField] public bool isSelected = false;
    [SerializeField] public bool hasItem = false;
    [SerializeField] public Image slotImage;
    [SerializeField] public Image icon;
    [SerializeField] public ItemData itemData;

    public GameObject renderModel;
    
    public static event Action<InventorySlot> OnInventorySlotSubmit;
    public static event Action<InventorySlot> OnInventorySlotMove;

    private Color defaultColor = new Color(255, 255, 255, 0.1294118f);
    private Color selectedColor = new Color(255, 255, 255, 1f);
    public void SetDefaultColor() => slotImage.color = defaultColor;
    public void SetSelectedColor() => slotImage.color = selectedColor;

    //Initialize Slot
    public void Set(ItemData _itemData)
    {
        itemData = _itemData;
        icon.sprite = itemData.icon;
        icon.enabled = true;
        renderModel = itemData.renderModel;
        hasItem = true;
    }

    //Remove Slot
    public void Remove()
    {
        icon = null;
        icon.enabled = false;
        renderModel = null;
        hasItem = false;
    }

    public void OnMove(AxisEventData eventData)
    {
        isSelected = eventData.selectedObject == this.gameObject ? true : false;
        var invSlot = eventData.selectedObject.GetComponent<InventorySlot>();

        //Fire OnInventorySlotMove event if button has a inv slot
        if (invSlot)
            OnInventorySlotMove?.Invoke(invSlot);
        else
            UIManager.Singleton.ClearInventoryData();
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
        //UIManager.Singleton.inventoryAudio.PlayActionMenuOpen();

        OnInventorySlotSubmit?.Invoke(this);
    }
}
