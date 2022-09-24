using RiptideNetworking;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{   
    private static ushort nextSpawnerId = 1;
    public ushort spawnerItemId;
    public ushort Id;
    public bool hasItem = false;   

    private void Start()
    {
        //Initialize ItemID from Collectable
        var item = GetComponentInChildren<Item>();

        if(item != null)
        {
            spawnerItemId = item.itemData.itemId;
        }
        
        //Initialize ItemSpawner
        hasItem = true;
        Id = nextSpawnerId;
        nextSpawnerId++;

        GameLogic.itemSpawners.Add(Id,this);
    }


    public void ItemPickUp()
    {
        ItemPickedUp(Id);
        hasItem = false;
        GameLogic.itemSpawners.Remove(Id);
    }

 
    #region Message Senders

    public void SpawnItem( ushort Id, Vector3 spawnerPos, bool hasItem, ushort itemId)
    {
        Message message = Message.Create(MessageSendMode.reliable, ServerToClientId.createItemSpawner);
        message.AddUShort(Id);
        message.AddVector3(spawnerPos);
        message.AddBool(hasItem);
        message.AddUShort(itemId);

        NetworkManager.Singleton.Server.SendToAll(message);
    }

    public static void ItemPickedUp(ushort _spawnerId)
    {
        Message message = Message.Create(MessageSendMode.reliable, ServerToClientId.itemPickedUp);
        message.AddUShort(_spawnerId);

        NetworkManager.Singleton.Server.SendToAll(message);
    }

    #endregion



}
