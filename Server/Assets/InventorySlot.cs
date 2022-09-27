using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : Button
{
    [SerializeField] public ushort id;
    [SerializeField] public bool isSelected = false;
    [SerializeField] public bool hasItem = false;
    [SerializeField] public Image slotImage;
    [SerializeField] public Item item;
    [SerializeField] public TextMeshProUGUI itemName;

    public EventSystem eventSystem;

    public static event Action<InventorySlot> OnInventorySlotSubmit;

    private Color defaultColor = new Color(255, 255, 255, 0.3490196f);
    private Color selectedColor = new Color(255, 255, 255, 1f);

    protected override void Awake()
    {
        eventSystem = GetComponentInParent<PlayerInventory>().eventSystem;
        base.Awake();    
    }

    public override void Select()
    {
        if (eventSystem.alreadySelecting)
            return;

        eventSystem.SetSelectedGameObject(gameObject);
        isSelected = true;
    }

    public void DeSelect()
    {
        isSelected = false;
    }

    public void Set(Item _item)
    {
        item = _item;
        itemName.text = _item.itemData.itemName;
        hasItem = true;
    }

    public void Remove()
    {
        item = null;
        itemName.text = String.Empty;
        hasItem = false;
    }

    public override void OnSelect(BaseEventData eventData)
    {
        isSelected = eventData.selectedObject == this.gameObject ? true : false;
        slotImage.color = selectedColor;
    }
     
    public override void OnDeselect(BaseEventData eventData)
    {
        slotImage.color = defaultColor;
    }

    public override void OnSubmit(BaseEventData eventData)
    {
        var invSlot = eventData.selectedObject.GetComponent<InventorySlot>();
        OnInventorySlotSubmit?.Invoke(invSlot);
    }
}
