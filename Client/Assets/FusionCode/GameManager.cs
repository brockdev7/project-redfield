using Fusion;
using ProjectRedfield.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : NetworkBehaviourSingleton<GameManager>, ISpawned
{
    [SerializeField] private UI_Game gameUi;

    public static UnityEvent OnGameStart = new UnityEvent();
    public static UnityEvent<FusionPlayer> OnGameEnd = new UnityEvent<FusionPlayer>();

    [Networked(OnChanged = nameof(OnGamePlayingChanged))] public NetworkBool gamePlaying { get; set; } = false;
    public static bool GamePlaying => Instance.gamePlaying;
    public const float StartDelay = 3f;

    public static void OnGamePlayingChanged(Changed<GameManager> changed)
    {
        if (changed.Behaviour.gamePlaying)
            changed.Behaviour.HandleOnGamePlayingChanged();
    }

    private void HandleOnGamePlayingChanged()
    {
        OnGameStart?.Invoke();
    }

    public override void Spawned()
    {
        base.Spawned();

        Debug.Log($"GameManager Spawned - State Auth {Object.HasStateAuthority}");
        if (!Instance.Object.HasStateAuthority) return;

        StartCoroutine(SetupGame());
    }

    private IEnumerator SetupGame()
    {
        Debug.Log($"GameManager SetupGame called");
        yield return new WaitForEndOfFrame();

        foreach (var player in App.Players)
        {
            SetupPlayer(player);
        }

        yield return new WaitForSecondsRealtime(1f);

        if (_playersSetup == 2)
            StartGame();
        else
        {
            //handle edge case where we dont have 2 players here, possibly a disconnect during scene change
            UI_StatusText.SetStatusText($"Error - Unable to start game");
            yield return new WaitForSecondsRealtime(1f);
            App.Instance.Disconnect();
        }
    }

    private int _playersSetup = 0;

    private void SetupPlayer(FusionPlayer player)
    {
        if (!App.IsMaster &&
            (Runner.Topology != SimulationConfig.Topologies.Shared || player.Object.InputAuthority != Runner.LocalPlayer)) return;

        Debug.Log($"GameManager::HandlePlayerJoined called - PlayerRef {player.Object.InputAuthority}");


        //if (_playersSetup == 0)
        //{
        //    var pos = Random.value > 0.5f ? PlayerPosition.Left : PlayerPosition.Right;
        //    Debug.Log($"setting player {player.PlayerNumber} to pos {pos}");
        //    player.PlayerPosition = pos;
        //}
        //else
        //{
        //    //second player to join, get OTHER paddle
        //    var otherPos = App.Instance.GetOtherPlayer(player.Object.InputAuthority).PlayerPosition;
        //    var pos = otherPos == PlayerPosition.Left ? PlayerPosition.Right : PlayerPosition.Left;
        //    Debug.Log($"setting player {player.PlayerNumber} to pos {pos}");
        //    player.PlayerPosition = pos;
        //}
        _playersSetup++;
    }

    private void HandlePlayerLeft(PlayerRef playerRef)
    {
        //player disconnected, end game with the remaining player as the winner
        UI_StatusText.SetStatusText($"PlayerRef {playerRef} Disconnected");
    }

    private void StartGame()
    {
        Debug.Log($"GameManager::StartGame called");

        if (!Instance.Object.HasStateAuthority) return;

        foreach (var player in App.Players)
        {
            //player.ResetPlayer();
        }

        //Instance.ball.ResetBall(startDelay: StartDelay);

        gamePlaying = true;
    }

    public static FusionPlayer GetPlayer(PlayerRef playerRef)
    {
        return App.Instance.GetPlayer(playerRef);
    }

    public static FusionPlayer GetLocalPlayer()
    {
        return App.Instance.GetLocalPlayer();
    }



}
