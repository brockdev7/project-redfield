using RiptideNetworking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{   
    public static Dictionary<int,ItemSpawner> spawners = new Dictionary<int, ItemSpawner>();
    
    private static int nextSpawnerId = 1;
    public int spawnerItemId;
    public int spawnerId;
    public bool hasItem = false;

    private void Start()
    {
        //Initialize ItemID from Collectable
        var item = GetComponentInChildren<Collectable>();

        if(item != null)
        {
            spawnerItemId = item.ItemId;
        }
        
        //Initialize ItemSpawner
        hasItem = true;
        spawnerId = nextSpawnerId;
        nextSpawnerId++;
        spawners.Add(spawnerId,this);
    }

    public void OnEnable()
    {
        Collectable.OnItemCollected += ItemPickUp;
    }

    private void ItemPickUp()
    {      
        hasItem = false;
        ItemPickedUp(spawnerId);
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
