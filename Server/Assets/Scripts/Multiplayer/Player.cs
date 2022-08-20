using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RiptideNetworking;

public class Player : MonoBehaviour
{
    public static Dictionary<ushort, Player> list = new Dictionary<ushort, Player>();
    public static List<Collectable> inventory = new List<Collectable>();

    public ushort Id { get; private set; }
    public string Username { get; private set; }

    public int itemAmount = 0;
    public int maxItemAmount = 4;
    public bool isPickingUpItem = false;

    public PlayerMovement Movement => movement;

    [SerializeField] private PlayerMovement movement;

    private void OnDestroy()
    {        
        list.Remove(Id);
    }

    #region Player Methods

    public bool AttemptingItemPickup()
    {       
        if(this.Movement.isPressed("E"))
        {
            if (itemAmount >= maxItemAmount)
                return false;

            this.Movement.DisableMovement();
            isPickingUpItem = true;
            PickingUpItem();

            return true;
        }

        return false;
    }

    public void ExitItemPickupAnim(int _itemId)
    {
        if(isPickingUpItem)
        {
            if (GameLogic.ItemList.TryGetValue(_itemId, out Collectable item))
            {
                inventory.Add(item);
                Debug.Log($"{item.DisplayName} has been added to the inventory.");
            }

            isPickingUpItem = false;
            this.Movement.EnableMovement();
            ExitItemPickup();
        }
    }

    public static void SpawnPlayer(ushort id, string username)
    {
        //Sends existing players spawn data to newly connected client
        foreach (Player otherPlayer in list.Values)
        {
            otherPlayer.SendSpawned(id);
        }

        Player player = Instantiate(GameLogic.Singleton.PlayerPrefab, new Vector3(9.4f, 1f, 9.4f), Quaternion.identity).GetComponent<Player>();
        player.name = $"Player {id} ({(string.IsNullOrEmpty(username) ? "Guest" : username)})";
        player.Id = id;
        player.Username = string.IsNullOrEmpty(username) ? $"Guest: {id}" : username;

        //Sends newly connected client's spawn data to all connected clients
        player.SendSpawned();

        //Send Server ItemSpawner data to client
        foreach (ItemSpawner _itemSpawner in ItemSpawner.spawners.Values)
        {
            _itemSpawner.SpawnItem(_itemSpawner.spawnerId, _itemSpawner.transform.position, _itemSpawner.hasItem, _itemSpawner.spawnerItemId);
        }

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


    //PLAYER
    //ACTIONS
    private void PickingUpItem()
    {
        Message message = Message.Create(MessageSendMode.reliable, ServerToClientId.playerPickingItemUp);
        message.AddInt(Id);
        NetworkManager.Singleton.Server.SendToAll(message);
    }

    private void ExitItemPickup()
    {
        Message message = Message.Create(MessageSendMode.reliable, ServerToClientId.playerExitItemAnimation);
        message.AddInt(Id);
        NetworkManager.Singleton.Server.SendToAll(message);
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
            player.Movement.SetInput(message.GetBools(7));         
        }
           
    }

    #endregion


}
