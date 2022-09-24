using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RiptideNetworking;

public class ItemSpawner : MonoBehaviour
{
    public ushort spawnerId;
    public bool hasItem;
    public MeshRenderer itemModel;
    public ParticleSystem glowEffect;

    public void Initialize(ushort _spawnerId, bool _hasItem)
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
        ushort _spawnerId = message.GetUShort();
        Vector3 _spawnerPos = message.GetVector3();
        bool _hasItem = message.GetBool();
        ushort _itemId = message.GetUShort();

        GameLogic.Singleton.SpawnItem(_spawnerId, _spawnerPos, _hasItem,_itemId);        
    }

    [MessageHandler((int)ServerToClientId.itemPickedUp)]
    private static void ItemPickedUp(Message message)
    {
        var spawnerId = message.GetUShort();

        //Pick Up Item Spawner
        if (GameLogic.itemSpawners.TryGetValue(spawnerId, out ItemSpawner spawner))
            spawner.ItemPickedUp();
     
    }

    #endregion



}
