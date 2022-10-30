using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RiptideNetworking;
using System.Linq;

public class Player : MonoBehaviour
{
    public ushort Id { get; private set; }
    public string Username { get; private set; }
    public PlayerStance PlayerStance { get; private set; }
    public PlayerLocomotionMode LocomotionMode { get; private set; }

    [SerializeField] public float HealthPoints = 100f;

    public static Dictionary<ushort, Player> list = new Dictionary<ushort, Player>();
    public  List<Player> otherPlayers
    {
        get { return list.Values.Where(x => x.Id != this.Id).ToList(); }
    }

    public PlayerMovement Movement => movement;
    public PlayerInventory Inventory => inventory;

    [SerializeField]  private PlayerInventory inventory;
    [SerializeField]  private PlayerMovement movement;

    public void Awake()
    {
        PlayerStance = PlayerStance.Idle;
    }

    private void OnDestroy()
    {        
        list.Remove(Id);
    }

    #region Player Methods

    public void SetAttackStance(PlayerStance stance)
    {
        PlayerStance = stance;
    }

    public void SetLocomotionMode(PlayerLocomotionMode mode)
    {
        LocomotionMode = mode;
    }
    #endregion






    #region Riptide UDP

    public static void SpawnPlayer(ushort id, string username)
    {
        //Send Server ItemSpawner data to client
        foreach (ItemSpawner _itemSpawner in GameLogic.itemSpawners.Values)
            _itemSpawner.SpawnItem(_itemSpawner.Id, _itemSpawner.transform.position, _itemSpawner.hasItem, _itemSpawner.spawnerItemId);

        //New player receives any already connected clients spawn data
        foreach (Player otherPlayer in list.Values)
            otherPlayer.SendSpawnData(id);

        Player player = Instantiate(GameLogic.Singleton.PlayerPrefab, new Vector3(9.4f, 0f, 9.4f), Quaternion.identity).GetComponent<Player>();

        player.name = $"Player {id} ({(string.IsNullOrEmpty(username) ? "Guest" : username)})";
        player.Id = id;
        player.Username = string.IsNullOrEmpty(username) ? $"Guest: {id}" : username;

        //Sends newly connected client's spawn data to all connected clients
        player.SendSpawnData();


        list.Add(id, player);
    }


    #region Message Senders
    //Sends player spawn data to a specific client on the server
    public void SendSpawnData(ushort toClientId)
    {
        NetworkManager.Singleton.Server.Send(AddSpawnData(Message.Create(MessageSendMode.reliable, ServerToClientId.playerSpawned)), toClientId);
    }

    //Sends player spawn data to all players currently on the server
    public void SendSpawnData()
    {
        NetworkManager.Singleton.Server.SendToAll(AddSpawnData(Message.Create(MessageSendMode.reliable, ServerToClientId.playerSpawned)));
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

    #endregion

    #endregion


}
