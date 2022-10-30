using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InfoButton : MonoBehaviour, ISelectHandler, IDeselectHandler, IMoveHandler, ISubmitHandler
{
    [SerializeField] public bool isSelected = false;

    public static event Action<InventorySlot> OnInfoButtonMove;
    public static event Action OnInfoButtonSubmit;

    public void OnMove(AxisEventData eventData)
    {

        isSelected = eventData.selectedObject == this.gameObject ? true : false;

        var invSlot = eventData.selectedObject.GetComponent<InventorySlot>();

        if (invSlot)
            OnInfoButtonMove?.Invoke(invSlot);
        else
            UIManager.Singleton.ClearInventoryData();
        
    }

    public void OnSelect(BaseEventData eventData)
    {
        isSelected = eventData.selectedObject == this.gameObject ? true : false;
    }

    public void OnDeselect(BaseEventData eventData)
    {

    }

    public void OnSubmit(BaseEventData eventData)
    {
        Debug.Log(eventData.selectedObject.name);
        OnInfoButtonSubmit?.Invoke();
    }


}
