using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RiptideNetworking;

public class Player : MonoBehaviour
{
    public static Dictionary<ushort, Player> list = new Dictionary<ushort, Player>();

    public ushort Id { get; private set; }
    public string Username { get; private set; }


    public PlayerMovement Movement => movement;
    public PlayerInventory Inventory => inventory;


    [SerializeField] private PlayerInventory inventory;
    [SerializeField] private PlayerMovement movement;

    private void OnDestroy()
    {        
        list.Remove(Id);
    }

    #region Player Methods


    public static void SpawnPlayer(ushort id, string username)
    {
        //Send Server ItemSpawner data to client
        foreach (ItemSpawner _itemSpawner in GameLogic.itemSpawners.Values)
        {
            _itemSpawner.SpawnItem(_itemSpawner.spawnerId, _itemSpawner.transform.position, _itemSpawner.hasItem, _itemSpawner.spawnerItemId);
        }

        //Sends existing players spawn data to newly connected client
        foreach (Player otherPlayer in list.Values)
        {
            otherPlayer.SendSpawned(id);
        }

        Player player = Instantiate(GameLogic.Singleton.PlayerPrefab, new Vector3(9.4f, 0f, 9.4f), Quaternion.identity).GetComponent<Player>();
        
        player.name = $"Player {id} ({(string.IsNullOrEmpty(username) ? "Guest" : username)})";
        player.Id = id;
        player.Username = string.IsNullOrEmpty(username) ? $"Guest: {id}" : username;

        //Sends newly connected client's spawn data to all connected clients
        player.SendSpawned();


        list.Add(id, player);
    }

    #endregion


    #region Message Senders

    //SERVER
    //ACTIONS

    //Sends player spawn data to all players currently on the server
    private void SendSpawned()
    {
        NetworkManager.Singleton.Server.SendToAll(AddSpawnData(Message.Create(MessageSendMode.reliable, ServerToClientId.playerSpawned)));
    }

    //Sends player spawn data to a specific client on the server
    private void SendSpawned(ushort toClientId)
    {
        NetworkManager.Singleton.Server.Send(AddSpawnData(Message.Create(MessageSendMode.reliable, ServerToClientId.playerSpawned)),toClientId);
    }

    #endregion


    #region Utility Methods
    private Message AddSpawnData(Message message)
    {
        message.AddUShort(Id);
        message.AddString(Username);
        message.AddVector3(transform.position);
        return message;
    }

    #endregion


    #region Message Handlers

    //Spawns player from GameLogic prefab after Client connects to server
    [MessageHandler((ushort)ClientToServerId.playerLoggingIn)]
    private static void PlayerLoggingIn(ushort fromClientId, Message message)
    {
        SpawnPlayer(fromClientId, message.GetString());
    }

    [MessageHandler((ushort)ClientToServerId.input)]
    private static void Input(ushort fromClientId, Message message)
    {
        if (list.TryGetValue(fromClientId, out Player player))
        {
            player.Movement.SetInput(message.GetBools(10));         
        }
           
    }

    #endregion


}
