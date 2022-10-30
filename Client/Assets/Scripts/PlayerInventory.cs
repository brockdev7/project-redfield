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
            if (player.Id == NetworkManager.Singleton.Client.Id)
            {
                var itemId = message.GetUShort();
                var spawnerId = message.GetUShort();
                UIManager.Singleton.OpenInventoryScreen(itemId, spawnerId, player);
            }

            player.animationManager.EnterItemPickUp();         
        }
    }

    [MessageHandler((ushort)ServerToClientId.playerExitItemPickUp)]
    private static void PlayerExitItemPickUp(Message message)
    {
        if (Player.list.TryGetValue(message.GetUShort(), out Player player))
        {
            if (player.Id == NetworkManager.Singleton.Client.Id)
            {            
                UIManager.Singleton.SetViewMode(InventoryMode.view);
                UIManager.Singleton.CloseInventoryScreen();
            }

            player.animationManager.ExitItemPickUp();
        }
    }

    [MessageHandler((ushort)ServerToClientId.inventoryItemPistolEquipped)]
    private static void PistolEquipped(Message message)
    {
        if (Player.list.TryGetValue(message.GetUShort(), out Player player))
        {
            player.animationManager.EquipPistol();
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

            if (player.Id == NetworkManager.Singleton.Client.Id)
            {
                var slot = UIManager.Singleton.GetInventorySlot(slotId);

                //Update Slot Data
                if (slot)
                    slot.Set(itemData);

                UIManager.Singleton.SetViewMode(InventoryMode.view);
                UIManager.Singleton.CloseInventoryScreen();
            }
            else
            {
                UIManager.Singleton.AddToTeammateInventory(player.Id, itemData, slotId);
            }

            //Add Item To Inventory
            player.inventory.list.Add(itemData);
  
            //Exit Player Kneel Animation
            player.animationManager.ExitItemPickUp();       
        }
    }

    #endregion




}
