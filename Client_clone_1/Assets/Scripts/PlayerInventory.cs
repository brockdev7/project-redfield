using RiptideNetworking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UIManager;

public class PlayerInventory : MonoBehaviour
{

    private List<ItemData> list { get; set; } = new List<ItemData>();


    public void Add(ItemData _itemData)
    {
        list.Add(_itemData);
    }

    public void Remove(ItemData _itemData)
    {
        list.Remove(_itemData);
    }


    [MessageHandler((ushort)ServerToClientId.playerPickingItemUp)]
    private static void PlayerPickingItemUp(Message message)
    {
        if (Player.list.TryGetValue(message.GetUShort(), out Player player))
        {
            var itemId = message.GetUShort();
            var spawnerId = message.GetInt();

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
            var itemData = GameLogic.itemList[itemId];
            var selectedSlot = UIManager.Singleton.GetSelectedSlot();

            selectedSlot.SetItemData(itemData);
            UIManager.Singleton.AddToSlot(selectedSlot);
                    
            player.animationManager.ExitItemPickUp();

            UIManager.Singleton.CloseInventoryScreen();
        }
    }




}
