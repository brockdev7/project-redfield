using RiptideNetworking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public List<Item> inventory = new List<Item>();

    public int itemAmount { get; set; }
    public int maxItemAmount { get; set; }
    public bool isPickingUpItem { get; set; }


    public void Awake()
    {
        itemAmount = 0;
        maxItemAmount = 4;
        isPickingUpItem = false;
    }

    public void AttemptItemPickup(Player _player,Item _item)
    {
        var _spawner = _item.GetComponentInParent<ItemSpawner>();

        //Max Capacity
        if (itemAmount >= maxItemAmount)
            return;

        //Spawner
        if (!_spawner.hasItem)
            return;

        _player.Movement.DisableMovement();

        itemAmount++;
        isPickingUpItem = true;

        PickingUpItem(_player.Id,_item.itemData.itemId);
    }

    public void ExitItemPickupAnim(Player _player,Item _item)
    {
        if (isPickingUpItem)
        {
            var spawner = _item.GetComponentInParent<ItemSpawner>();

            //Set flag & enable movement
            isPickingUpItem = false;
            _player.Movement.EnableMovement();
         
            //Fire Item Collected Event
            _item.Collect();

            //Add To Inventory
            inventory.Add(_item);
            Debug.Log($"{_player.name} has added {_item.itemData.itemName} to their inventory.");

            //Spawner State Update
            spawner.ItemPickUp();

            ExitItemPickup(_player.Id);
        }
    }

    private void PickingUpItem(ushort _playerId,ushort _itemId)
    {
        Message message = Message.Create(MessageSendMode.reliable, ServerToClientId.playerPickingItemUp);
        message.AddUShort(_playerId);
        message.AddUShort(_itemId);
        NetworkManager.Singleton.Server.Send(message, _playerId);
    }

    private void ExitItemPickup(ushort _playerId)
    {
        Message message = Message.Create(MessageSendMode.reliable, ServerToClientId.playerExitItemAnimation);
        message.AddUShort(_playerId);
        NetworkManager.Singleton.Server.Send(message, _playerId);
    }


}

