using Steamworks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SteamLobby : MonoBehaviour
{
    private static SteamLobby _singleton;
    internal static SteamLobby Singleton
    {
        get => _singleton;
        private set
        {
            if (_singleton == null)
                _singleton = value;
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(SteamLobby)} instance already exists, destroying object!");
                Destroy(value);
            }
        }
    }

    protected Callback<LobbyCreated_t> lobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;
    protected Callback<LobbyEnter_t> lobbyEnter;

    protected Callback<LobbyMatchList_t> lobbyList;
    protected Callback<LobbyDataUpdate_t> lobbyDataUpdated;

    public List<CSteamID> lobbyIds = new List<CSteamID>();

    private const string HostAddressKey = "HostAddress";
    private CSteamID lobbyId;

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

        lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
        lobbyEnter = Callback<LobbyEnter_t>.Create(OnLobbyEnter);
        lobbyList = Callback<LobbyMatchList_t>.Create(OnGetLobbyList);
        lobbyDataUpdated = Callback<LobbyDataUpdate_t>.Create(OnGetLobbyData);
    }

    internal void CreateLobby()
    {
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, 4);
    }

    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        if (callback.m_eResult != EResult.k_EResultOK)
        {
            UIManager.Singleton.LobbyCreationFailed();
            return;
        }

        lobbyId = new CSteamID(callback.m_ulSteamIDLobby);
        string steamPersona = SteamFriends.GetPersonaName();
        SteamMatchmaking.SetLobbyData(lobbyId, HostAddressKey, SteamUser.GetSteamID().ToString());
        SteamMatchmaking.SetLobbyData(lobbyId, "name", $"{steamPersona}'s lobby");


        UIManager.Singleton.LobbyCreationSucceeded();
        App.Instance.StartRoomGame(Fusion.GameMode.Host,steamPersona);   
    }

    internal void JoinLobby(CSteamID lobbyId)
    {
        SteamMatchmaking.JoinLobby(lobbyId);
    }

    private void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback)
    {
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    internal void LeaveLobby()
    {
        SteamMatchmaking.LeaveLobby(lobbyId);
        App.Instance.Disconnect();      
    }

    private void OnLobbyEnter(LobbyEnter_t callback)
    {
        lobbyId = new CSteamID(callback.m_ulSteamIDLobby);
        string lobbyName = SteamMatchmaking.GetLobbyData(lobbyId, "name");

        UIManager.Singleton.SetLobbyData(lobbyName);
        UIManager.Singleton.LobbyEntered();
        App.Instance.StartRoomGame(Fusion.GameMode.Client, lobbyName);
    }

    public void SearchLobbies(string text)
    {
        if (lobbyIds.Count > 0) { lobbyIds.Clear(); }
        SteamMatchmaking.AddRequestLobbyListResultCountFilter(60);
        SteamMatchmaking.AddRequestLobbyListStringFilter("name", text, ELobbyComparison.k_ELobbyComparisonEqual);
        SteamMatchmaking.RequestLobbyList();
    }

    public void GetLobbiesList()
    {
        if(lobbyIds.Count > 0) { lobbyIds.Clear(); }

        SteamMatchmaking.AddRequestLobbyListResultCountFilter(60);
        SteamMatchmaking.RequestLobbyList();
    }
    internal void OnGetLobbyList(LobbyMatchList_t result)
    {
        if(UIManager.Singleton.lobbyRoomContainers.Count > 0) 
        {
            UIManager.Singleton.DestroyLobbies(); 
        }

        for(int i = 0; i < result.m_nLobbiesMatching; i++)
        {
            CSteamID lobbyId = SteamMatchmaking.GetLobbyByIndex(i);
            lobbyIds.Add(lobbyId);
            SteamMatchmaking.RequestLobbyData(lobbyId);
        }
    }

    internal void OnGetLobbyData(LobbyDataUpdate_t result)
    {
        UIManager.Singleton.DisplayLobbies(lobbyIds,result);
    }








}

