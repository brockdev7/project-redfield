using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RiptideNetworking;
using static PlayerAnimationManager;

public class Player : MonoBehaviour
{

    public static Dictionary<ushort, Player> list = new Dictionary<ushort, Player>();

    public ushort Id { get; private set; }
    public bool IsLocal { get; private set; }

    [SerializeField] public PlayerAnimationManager animationManager;
    [SerializeField] public PlayerInventory inventory;
    [SerializeField] private Interpolator interpolator;
    
    private string username;

    public void OnDestroy()
    {
        list.Remove(Id);
    }

    private void Move(ushort tick, Vector3 newPosition, Quaternion newRotation)
    {
        interpolator.NewPositionUpdate(tick, newPosition);
        interpolator.NewRotationUpdate(tick, newRotation);    
    }

    public static void Spawn(ushort id, string username, Vector3 position)
    {
        Player player;

        if(id == NetworkManager.Singleton.Client.Id)
        {
            player = Instantiate(GameLogic.Singleton.LocalPlayerPrefab, position, Quaternion.identity).GetComponent<Player>();
            player.IsLocal = true;
        }
        else
        {
            player = Instantiate(GameLogic.Singleton.PlayerPrefab, position, Quaternion.identity).GetComponent<Player>();
            player.IsLocal = false;
        }

        player.name = $"Player {id} ({(string.IsNullOrEmpty(username) ? "Guest" : username)})";
        player.Id = id;
        player.username = username;

        list.Add(id, player);   
    }


    #region Message Handlers

    [MessageHandler((ushort)ServerToClientId.playerSpawned)]
    private static void SpawnPlayer(Message message)
    {
        Spawn(message.GetUShort(), message.GetString(), message.GetVector3());
        UIManager.Singleton.AssignTeamFrames();           
    }

    [MessageHandler((ushort)ServerToClientId.playerMovement)]
    private static void PlayerMovement(Message message)
    {
        if (list.TryGetValue(message.GetUShort(), out Player player))
            player.Move(message.GetUShort(),message.GetVector3(), message.GetQuaternion());
    }


    #endregion


}
