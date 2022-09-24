using RiptideNetworking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UIManager;

public class PlayerInventory : MonoBehaviour
{
    [SerializeField] public List<ItemData> list = new List<ItemData>();

    #region Message Handlers
    [MessageHandler((ushort)ServerToClientId.playerPickingItemUp)]
    private static void PlayerPickingItemUp(Message message)
    {
        if (Player.list.TryGetValue(message.GetUShort(), out Player player))
        {
            var itemId = message.GetUShort();
            var spawnerId = message.GetUShort();

            player.animationManager.EnterItemPickUp();

            UIManager.Singleton.OpenInventoryScreen(itemId,spawnerId, player);
        }
    }

    [MessageHandler((ushort)ServerToClientId.playerExitItemPickUp)]
    private static void PlayerExitItemPickUp(Message message)
    {
        if (Player.list.TryGetValue(message.GetUShort(), out Player player))
        {
            player.animationManager.ExitItemPickUp();
            UIManager.Singleton.SetViewMode(InventoryMode.view);
            UIManager.Singleton.CloseInventoryScreen();
        }
    }

    [MessageHandler((ushort)ServerToClientId.inventoryItemAdded)]
    private static void InventoryItemAdded(Message message)
    {
        if (Player.list.TryGetValue(message.GetUShort(), out Player player))
        {
            var itemId = message.GetUShort();
            var itemData = GameLogic.Singleton.GetItemData(itemId);
            var slotId = message.GetUShort();

            var slot = UIManager.Singleton.GetInventorySlot(slotId);

            //Update Slot Data
            if(slot)
                slot.Set(itemData);

            //Add Item To Inventory
            player.inventory.list.Add(itemData);
  
            //Exit Player Kneel Animation
            player.animationManager.ExitItemPickUp();

            UIManager.Singleton.SetViewMode(InventoryMode.view);
            UIManager.Singleton.CloseInventoryScreen();          
        }
    }

    [MessageHandler((ushort)ServerToClientId.inventoryUpdate)]
    private static void InventoryUpdate(Message message)
    {    
        if (Player.list.TryGetValue(message.GetUShort(), out Player teammate))
        {
            var itemId = message.GetUShort();
            var itemData = GameLogic.Singleton.GetItemData(itemId);
            var slotId = message.GetUShort();

            teammate.inventory.list.Add(itemData);
            UIManager.Singleton.AddToTeammateInventory(teammate.Id,itemData,slotId);

            Debug.Log($"{teammate.gameObject.name} has added a {itemData.itemName} to their inventory.");
        }      
    }
    #endregion




}
