using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RiptideNetworking;


public class Player : MonoBehaviour
{
    public static Dictionary<ushort, Player> list = new Dictionary<ushort, Player>();

    public ushort Id { get; private set; }
    public bool IsLocal { get; private set; }

    [SerializeField] private PlayerAnimationManager animationManager;
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private Interpolator interpolator;
    
    private string username;

    public void OnDestroy()
    {
        list.Remove(Id);
    }

    private void Move(ushort tick, Vector3 newPosition, Quaternion newRotation, float playerSpeed)
    {
        interpolator.NewPositionUpdate(tick, newPosition);
        interpolator.NewRotationUpdate(tick, newRotation);
        animationManager.Animate(playerSpeed);
    }

    private void SetRotation(Quaternion _newRotation)
    {
        transform.rotation = _newRotation;
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
    }

    [MessageHandler((ushort)ServerToClientId.playerMovement)]
    private static void PlayerMovement(Message message)
    {
        if (list.TryGetValue(message.GetUShort(), out Player player))
            player.Move(message.GetUShort(),message.GetVector3(), message.GetQuaternion(),message.GetFloat());
    }

    [MessageHandler((ushort)ServerToClientId.playerRotation)]
    private static void PlayerRotation(Message message)
    {
        if (list.TryGetValue(message.GetUShort(), out Player player))
            player.SetRotation(message.GetQuaternion());
    }



    #endregion


}
