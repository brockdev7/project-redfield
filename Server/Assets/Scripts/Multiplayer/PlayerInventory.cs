using RiptideNetworking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public List<Item> list = new List<Item>();
    public int itemAmount { get; set; }
    public bool isPickingUpItem { get; set; }

    public void AddToInventory(ushort _playerId,Item _item)
    {
        Debug.Log($"Player added {_item.itemData.itemName} to their inventory.");

        //Take Item from Spawner
        var itemSpawner = _item.GetComponentInParent<ItemSpawner>();
        itemSpawner.ItemPickUp();

        //Add Item to Inventory
        list.Add(_item);

        isPickingUpItem = false;
        AddedToInventory(_playerId, _item.itemData.itemId);
    }

    public void RemoveFromInventory(Item _item)
    {
        Debug.Log($"Player added {_item.itemData.itemName} to their inventory.");
        list.Remove(_item);
    }

    public void Awake()
    {
        itemAmount = 0;
        isPickingUpItem = false;
    }

    public void AttemptItemPickup(Player _player,Item _item)
    {
        var _spawner = _item.GetComponentInParent<ItemSpawner>();

        //Spawner
        if (!_spawner.hasItem)
            return;

        _player.Movement.DisableMovement();
        isPickingUpItem = true;

        PickingUpItem(_item.itemData.itemId, _spawner.Id);
    }


    #region Message Senders

    private void PickingUpItem(ushort _itemId, int _spawnerId)
    {
        var player = this.gameObject.GetComponent<Player>();

        Message message = Message.Create(MessageSendMode.reliable, ServerToClientId.playerPickingItemUp);
        message.AddUShort(player.Id);
        message.AddUShort(_itemId);
        message.AddInt(_spawnerId);
        NetworkManager.Singleton.Server.Send(message, player.Id);
    }

    public void ExitItemPickup()
    {
        var player = this.gameObject.GetComponent<Player>();
        isPickingUpItem = false;

        Message message = Message.Create(MessageSendMode.reliable, ServerToClientId.playerExitItemPickUp);
        message.AddUShort(player.Id);
        NetworkManager.Singleton.Server.Send(message, player.Id);
    }

    private void AddedToInventory(ushort _playerId, ushort _itemId)
    {
        Message message = Message.Create(MessageSendMode.reliable, ServerToClientId.inventoryItemAdded);
        message.AddUShort(_playerId);
        message.AddUShort(_itemId);
        NetworkManager.Singleton.Server.Send(message, _playerId);
    }


    #endregion





}

