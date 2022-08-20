using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class InventoryItem 
{
    public ItemData itemData;

    public InventoryItem(ItemData _itemData)
    {
        itemData = _itemData;
    }
}
