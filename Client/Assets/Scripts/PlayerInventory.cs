using RiptideNetworking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

            var animator = player.GetComponentInChildren<Animator>();
            animator.Play("KneelDown");

            UIManager.Singleton.OpenInventoryScreen(itemId);
        }
    }

    [MessageHandler((ushort)ServerToClientId.playerExitItemAnimation)]
    private static void PlayerExitItemAnimation(Message message)
    {
        if (Player.list.TryGetValue(message.GetUShort(), out Player player))
        {
            var animator = player.GetComponentInChildren<Animator>();
            animator.Play("KneelUp");

            UIManager.Singleton.CloseInventoryScreen();

        }
    }
}
