using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RiptideNetworking;

public class ItemSpawner : MonoBehaviour
{
    public int spawnerId;
    public bool hasItem;
    public MeshRenderer itemModel;
    public ParticleSystem glowEffect;

    public void Initialize(int _spawnerId, bool _hasItem)
    {
        spawnerId = _spawnerId;
        hasItem = _hasItem;
        itemModel.enabled = _hasItem;
    }

    public void ItemPickedUp()
    {
        hasItem = false;
        itemModel.enabled = false;
        glowEffect.gameObject.SetActive(false);
    }

    #region Message Handlers

    [MessageHandler((ushort)ServerToClientId.createItemSpawner)]
    private static void SpawnItem(Message message)
    {
        int _spawnerId = message.GetInt();
        Vector3 _spawnerPos = message.GetVector3();
        bool _hasItem = message.GetBool();
        int _itemId = message.GetInt();

        GameLogic.Singleton.SpawnItem(_spawnerId, _spawnerPos, _hasItem,_itemId);        
    }

    [MessageHandler((int)ServerToClientId.itemPickedUp)]
    private static void ItemPickedUp(Message message)
    {
        var spawnerId = message.GetInt();

        //Pick Up Item Spawner
        if (GameLogic.itemSpawners.TryGetValue(spawnerId, out ItemSpawner spawner))
            spawner.ItemPickedUp();
     
    }

    #endregion



}
