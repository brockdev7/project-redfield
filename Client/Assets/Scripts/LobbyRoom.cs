using Steamworks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyRoom : MonoBehaviour
{
    public CSteamID lobbyId;
    public string lobbyName;
    public TextMeshProUGUI lobbyNameText;

    public void SetLobbyData()
    {
        if (string.IsNullOrEmpty(lobbyName))
            lobbyNameText.text = "<No Lobby Name>";
        else
            lobbyNameText.text = lobbyName;
    }

    public void JoinLobby()
    {
        SteamLobby.Singleton.JoinLobby(lobbyId);
    }
}
