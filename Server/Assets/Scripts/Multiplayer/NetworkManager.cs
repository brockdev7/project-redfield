using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RiptideNetworking;
using RiptideNetworking.Utils;

public enum ServerToClientId : ushort
{
    playerSpawned = 1,
    playerMovement,
    playerRotation,
    playerPickingItemUp,
    playerExitItemAnimation,
    menuInventoryOpened,
    createItemSpawner,
    itemSpawned,
    itemPickedUp,
    sync,
}

public enum ClientToServerId : ushort
{
    playerLoggingIn = 1,
    input,
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

    public Server Server { get; private set; }
    public ushort CurrentTick { get; private set; } = 0;
    private bool firstSync = true;

    [SerializeField] private ushort port;
    [SerializeField] private ushort maxClientCount;

    private void Awake()
    {
        Singleton = this;
    }

    private void Start()
    {
        Application.targetFrameRate = 60;

        RiptideLogger.Initialize(Debug.Log, Debug.Log, Debug.LogWarning, Debug.LogError, false);
        Server = new Server();
        Server.Start(port, maxClientCount);
        Server.ClientDisconnected += PlayerLeft;
    }

    
    private void FixedUpdate()
    {
        Server.Tick();
        SendSync();

        CurrentTick++;
    }

    private void OnApplicationQuit()
    {
        Server.Stop();
    }

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
    

}



