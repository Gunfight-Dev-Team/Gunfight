using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Steamworks;

public class LobbyDataEntry : MonoBehaviour
{
    public CSteamID lobbyID;
    public string lobbyName;
    public Text lobbyNameText;
    public Text lobbyPlayerCountText;
    public int lobbyPlayerCount;

    public void SetLobbyData()
    {
        if (lobbyName == "")
        {
            lobbyNameText.text = "Empty";
        }
        else
        {
            lobbyNameText.text = lobbyName;
        }

        lobbyPlayerCountText.text = lobbyPlayerCount.ToString() + "/4";
    }

    public void JoinLobby()
    {
        SteamLobby.Instance.isJoining = true;
        SteamLobby.Instance.JoinLobby(lobbyID);
    }
}
