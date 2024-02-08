using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonManager : MonoBehaviour
{
    public Text ButtonInfo;

    [SerializeField]
    private string[] Infos =
    {
        "",
        "Quick Start and Dive into the Action!",
        "Create a Lobby and Invite Players",
        "Join the Fun and Explore Existing Lobbies!",
        "Test Your Skills with Single-Player Mode!"
    };

    public void HostLobbyButton()
    {
        Invoke("loadHostButton", 0.75f);
    }

    private void loadHostButton()
    {
        SteamLobby.Instance.HostLobby();
        PlayerPrefs.SetInt("isJoinedSingle", 0);
    }

    public void QuickStartButton()
    {
        Invoke("loadQuickStart", 0.75f);
    }

    private void loadQuickStart()
    {
        SteamLobby.Instance.QuickStart();
        PlayerPrefs.SetInt("isJoinedSingle", 0);
    }

    public void SinglePlayerButton()
    {
        Invoke("loadSinglePlayer", 0.75f);
    }

    private void loadSinglePlayer()
    {
        SteamLobby.Instance.HostLobby();
        PlayerPrefs.SetInt("isJoinedSingle", 1);
    }

    public void JoinLobbyButton()
    {
        Invoke("loadJoinLobby", 0.75f);
    }

    private void loadJoinLobby()
    {
        LobbiesListManager.instance.toLobbyList();
    }

    public void onHoverQuick()
    {
        ButtonInfo.text = Infos[1];
    }

    public void onHoverHost()
    {
        ButtonInfo.text = Infos[2];
    }

    public void onHoverJoin()
    {
        ButtonInfo.text = Infos[3];
    }

    public void onHoverSingle()
    {
        ButtonInfo.text = Infos[4];
    }

    public void onLeaveButton()
    {
        ButtonInfo.text = Infos[0];
    }
}
