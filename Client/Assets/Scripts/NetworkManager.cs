using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RiptideNetworking.Transports.SteamTransport;
using RiptideNetworking.Utils;
using System;
using RiptideNetworking;
using Steamworks;

public enum TransportMode
{
    LocalMp,
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
    // <-- Begin Riptide UDP -->
    #region Riptide UDP
    playerLoggingIn = 1,
    actionMenu_Equip,
    actionMenu_Use,
    input,
    #endregion
    // <-- End Riptide UDP -->

    // <-- Begin Steam Transport -->
    #region Steam 
    st_playerName
    #endregion

    // <-- End Steam Transport -->
}

public class NetworkManager : MonoBehaviour
{
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

    public Client Client { get; private set; }

    [SerializeField]  private string ip;
    [SerializeField]  private ushort port;
    [Space(10)]
    [SerializeField] private ushort tickDivergenceTolerance = 1;

    public TransportMode TransportMode { get; set; }

    private ushort _serverTick;

    public ushort ServerTick
    {
        get => _serverTick;
        private set
        {
            _serverTick = value;
            InterpolationTick = (ushort)(value - TicksBetweenPositionUpdates);
        }
    }

    public ushort InterpolationTick { get; private set; }
    private ushort _ticksBetweenPositionUpdates = 2;

    public ushort TicksBetweenPositionUpdates
    {
        get => _ticksBetweenPositionUpdates;
        private set
        {
            _ticksBetweenPositionUpdates = value;
            InterpolationTick = (ushort)(ServerTick - value);
        }
    }

    private void Awake()
    {
        Singleton = this;
    }

    private void Start()
    {
        if (!SteamManager.Initialized)
        {
            Debug.LogError("Steam is not initialized!");
            return;
        }

        RiptideLogger.Initialize(Debug.Log, Debug.Log, Debug.LogWarning, Debug.LogError, false);

       
        ServerTick = 2;
    }

    private void FixedUpdate()
    {
        if (TransportMode == TransportMode.LocalMp)
        {
            //Client.Tick();
            ServerTick++;
        }

        if(TransportMode == TransportMode.Steam)
        {
            //Client.Tick();
        }
    }

    private void OnApplicationQuit()
    {
        Client.Disconnect();
        Client.Connected -= DidConnect;
        Client.ConnectionFailed -= FailedToConnect;
        Client.ClientDisconnected -= PlayerLeft;
        Client.Disconnected -= DidDisconnect;
    }

    public void ConnectLocalMultiplayer()
    {
        Client = new Client();

        //Register Client Events
        Client.Connected += DidConnect;
        Client.ConnectionFailed += FailedToConnect;
        Client.ClientDisconnected += PlayerLeft;
        Client.Disconnected += DidDisconnect;

        Client.Connect($"{ip}:{port}");
    }

    public void ConnectSteam()
    {
        Client = new Client(new RiptideNetworking.Transports.SteamTransport.SteamClient());

        //Register Client Events
        Client.Connected += DidConnect;
        Client.ConnectionFailed += FailedToConnect;
        Client.ClientDisconnected += PlayerLeft;
        Client.Disconnected += DidDisconnect;
    }

    private void DidConnect(object sender, EventArgs e)
    {
        if(TransportMode == TransportMode.LocalMp)
        {
            UIManager.Singleton.LogIn();
        }

        if (TransportMode == TransportMode.Steam)
        {
            Message message = Message.Create(MessageSendMode.reliable, ClientToServerId.st_playerName);
            message.Add(Steamworks.SteamFriends.GetPersonaName());
            Client.Send(message);
        }
    }

    private void FailedToConnect(object sender, EventArgs e)
    {
        UIManager.Singleton.BackToMain();      
    }

    private void PlayerLeft(object sender, ClientDisconnectedEventArgs e)
    {
        if(Player.list.TryGetValue(e.Id, out Player player))
            Destroy(player.gameObject);
    }

    private void DidDisconnect(object sender, EventArgs e)
    {      
        foreach (Player player in Player.list.Values)
            Destroy(player.gameObject);

        Player.list.Clear();
        UIManager.Singleton.BackToMain();
    }

    private void SetTick(ushort serverTick)
    {
        if(Mathf.Abs(ServerTick - serverTick) > tickDivergenceTolerance)
        {
           // Debug.Log($"Client tick: {ServerTick} -> {serverTick}");
            ServerTick = serverTick;
        }
    }

    [MessageHandler((ushort)ServerToClientId.sync)]
    public static void Sync(Message message)
    {
        Singleton.SetTick(message.GetUShort());
    }


}
