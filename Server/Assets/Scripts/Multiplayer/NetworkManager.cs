using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RiptideNetworking;
using RiptideNetworking.Utils;
using RiptideNetworking.Transports.SteamTransport;
using Steamworks;

public enum TransportMode
{
    RiptideUdp,
    Steam
}


public enum ServerToClientId : ushort
{
    playerSpawned = 1,
    playerMovement,
    playerPickingItemUp,
    playerExitItemPickUp,
    inventoryItemAdded,
    inventoryItemPistolEquipped,
    menuInventoryOpened,
    createItemSpawner,
    itemSpawned,
    itemPickedUp,
    sync,
    animate
}

public enum ClientToServerId : ushort
{
    playerLoggingIn = 1,
    actionMenu_Equip,
    actionMenu_Use,
    input,
}


public class NetworkManager : MonoBehaviour
{
    public TransportMode TransportMode { get; set; }

    private static NetworkManager _singleton;

    public static NetworkManager Singleton
    {
        get => _singleton;
        private set
        {
            if (_singleton == null)
                _singleton = value;
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(NetworkManager)} instance already exists, destroying duplicate!");
                Destroy(value);
            }
        }
    }

    public const byte PlayerHostedDemoMessageHandlerGroupId = 255;

    public Server Server { get; private set; }
    public ushort CurrentTick { get; private set; } = 0;

    [SerializeField] private ushort port;
    [SerializeField] private ushort steamPort;
    [SerializeField] private ushort maxClientCount;

    private void Awake()
    {
        Singleton = this;
    }

    private void Start()
    {
        if (!SteamManager.Initialized)
        {
            RiptideLogger.Log(RiptideNetworking.Utils.LogType.debug,"Steam is initialized!");
            return;
        }

        Application.targetFrameRate = 60;
        TransportMode = TransportMode.Steam;

        RiptideLogger.Initialize(Debug.Log, Debug.Log, Debug.LogWarning, Debug.LogError, false);

        if(TransportMode == TransportMode.RiptideUdp)
        {
            Server = new Server();
            Server.Start(port, maxClientCount);
            Server.ClientDisconnected += PlayerLeft;
        }

        if(TransportMode == TransportMode.Steam)
        {
            SteamServer steamServer = new SteamServer();
            Server = new Server(steamServer);
            Server.ClientConnected += NewPlayerConnected;
            Server.ClientDisconnected += ServerPlayerLeft;



        }
    }
     
    
    private void FixedUpdate()
    {
        if(Server.IsRunning)
            Server.Tick();

        if (TransportMode == TransportMode.RiptideUdp)
        {
            //Were syncing on every frame instead of once every 5 seconds like in the tutorial
            //We can put this back if the server starts to perform badly (although obviously bad performance could be due to other reasons as well)
            SendSync();

            CurrentTick++;
        }
    }

    private void OnApplicationQuit()
    {
        Server.Stop();
    }


    // <-- Begin Steam Transport -->
    #region Steam Transport

    private void NewPlayerConnected(object sender, ServerClientConnectedEventArgs e)
    {
        foreach (Player player in Player.list.Values)
        {
            if (player.Id != e.Client.Id)
                 player.SendSpawnData(e.Client.Id);
        }
    }

    private void ServerPlayerLeft(object sender, ClientDisconnectedEventArgs e)
    {
        Destroy(Player.list[e.Id].gameObject);
    }

    #endregion
    // <-- End Steam Transport -->



    // <-- Begin Riptide Transport -->
    #region RiptideUDP Transport

    private void PlayerLeft(object sender, ClientDisconnectedEventArgs e)
    {
        if (Player.list.TryGetValue(e.Id, out Player player))
            Destroy(player.gameObject);
    }

    private void SendSync()
    {
        Message message = Message.Create(MessageSendMode.unreliable, (ushort)ServerToClientId.sync);
        message.Add(CurrentTick);
        Server.SendToAll(message);
    }

    #endregion
    // <--End Riptide Transport -->


}



