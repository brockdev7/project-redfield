using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public List<InventoryItem> inventory;

    public void Add(ItemData itemData)
    {
        InventoryItem newInventoryItem = new InventoryItem(itemData);
        inventory.Add(newInventoryItem);
    }

    public void Remove(InventoryItem item)
    {
        inventory.Remove(item);
    }

}
