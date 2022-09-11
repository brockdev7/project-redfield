using RiptideNetworking;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{   
    private static int nextSpawnerId = 1;
    public int spawnerItemId;
    public int Id;
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

    public void SpawnItem( int Id, Vector3 spawnerPos, bool hasItem, int itemId)
    {
        Message message = Message.Create(MessageSendMode.reliable, ServerToClientId.createItemSpawner);
        message.AddInt(Id);
        message.AddVector3(spawnerPos);
        message.AddBool(hasItem);
        message.AddInt(itemId);

        NetworkManager.Singleton.Server.SendToAll(message);
    }

    public static void ItemPickedUp(int _spawnerId)
    {
        Message message = Message.Create(MessageSendMode.reliable, ServerToClientId.itemPickedUp);
        message.AddInt(_spawnerId);

        NetworkManager.Singleton.Server.SendToAll(message);
    }

    #endregion



}
