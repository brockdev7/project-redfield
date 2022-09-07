using RiptideNetworking;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{   
    private static int nextSpawnerId = 1;
    public int spawnerItemId;
    public int spawnerId;
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
        spawnerId = nextSpawnerId;
        nextSpawnerId++;

        GameLogic.itemSpawners.Add(spawnerId,this);
    }


    public void ItemPickUp()
    {
        ItemPickedUp(spawnerId);
        hasItem = false;
        GameLogic.itemSpawners.Remove(spawnerId);
    }

 
    #region Message Senders

    public void SpawnItem( int spawnerId, Vector3 spawnerPos, bool hasItem, int itemId)
    {
        Message message = Message.Create(MessageSendMode.reliable, ServerToClientId.createItemSpawner);
        message.AddInt(spawnerId);
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
