using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Steamworks;
using UnityEngine.SceneManagement;

public class PlayerObjectController : NetworkBehaviour
{
    //Player Data
    [SyncVar] public int ConnectionID;
    [SyncVar] public int PlayerIdNumber;
    [SyncVar] public ulong PlayerSteamID;
    [SyncVar(hook = nameof(PlayerNameUpdate))] public string PlayerName;
    [SyncVar(hook = nameof(PlayerReadyUpdate))] public bool Ready;
    [SyncVar(hook = nameof(PlayerTeamUpdate))] public int Team = 1;
    public bool isAlive = true;

    //Cosmestics / Team
    [SyncVar(hook = nameof(SendPlayerColor))] public int PlayerColor;

    private CustomNetworkManager manager;

    private CustomNetworkManager Manager
    {
        get
        {
            if (manager != null)
            {
                return manager;
            }
            return manager = CustomNetworkManager.singleton as CustomNetworkManager;
        }
    }

    private void Start()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    private void PlayerReadyUpdate(bool oldValue, bool newValue)
    {
        if (isServer)
        {
            this.Ready = newValue;
        }
        if (isClient)
        {
            LobbyController.Instance.UpdatePlayerList();
        }
    }

    [Command]
    private void CMDSetPlayerReady()
    {
        this.PlayerReadyUpdate(this.Ready, !this.Ready);
    }

    public void ChangeReady()
    {
        if (isOwned)
        {
            CMDSetPlayerReady();
        }
    }

    private void PlayerTeamUpdate(int oldValue, int newValue)
    {
        if (isServer)
        {
            this.Team = newValue;
        }
        if (isClient)
        {
            LobbyController.Instance.UpdatePlayerList();
        }
    }

    [Command]
    private void CMDSetPlayerTeam(int newTeam)
    {
        this.PlayerTeamUpdate(this.Team, newTeam);
    }

    public void ChangeTeam(int newTeam)
    {
        if (isOwned)
        {
            CMDSetPlayerTeam(newTeam);
        }
    }

    public override void OnStartAuthority()
    {
        CmdSetPlayerName(SteamFriends.GetPersonaName().ToString());
        gameObject.name = "LocalGamePlayer";
        LobbyController.Instance.FindLocalPlayer();
        LobbyController.Instance.UpdateLobbyName();
        // why are we calling this twice?
        // LobbyController.Instance.UpdateLobbyName();
    }

    public override void OnStartClient()
    {
        Manager.GamePlayers.Add(this);
        LobbyController.Instance.UpdateLobbyName();
        LobbyController.Instance.UpdatePlayerList();
    }

    public override void OnStopClient()
    {
        Manager.GamePlayers.Remove(this);
        LobbyController.Instance.UpdatePlayerList();
    }

    [Command]
    private void CmdSetPlayerName(string PlayerName)
    {
        this.PlayerNameUpdate(this.PlayerName, PlayerName);
    }

    public void PlayerNameUpdate(string OldValue, string NewValue)
    {
        if (isServer)
        {
            this.PlayerName = NewValue;
        }
        if (isClient)
        {
            LobbyController.Instance.UpdatePlayerList();
        }
    }

    public void CanStartGame(string SceneName)
    {
        if (isOwned)
        {
            CmdCanStartGame(SceneName);
        }
    }

    [Command]
    public void CmdCanStartGame(string SceneName)
    {
        manager.StartGame(SceneName);
    }

    [Command]
    public void CmdUpdatePlayerColor(int newValue)
    {
        SendPlayerColor(PlayerColor, newValue);
    }

    public void SendPlayerColor(int oldValue, int newValue)
    {
        if(isServer)
        {
            PlayerColor = newValue;
        }
        if (isClient && (oldValue != newValue))
        {
            UpdateColor(newValue);
        }
    }

    //separte function because buggy if not
    void UpdateColor(int message)
    { 
        PlayerColor = message;
        GetComponent<PlayerController>().SwitchSkin(PlayerColor);
    }

    public void Quit()
    {
        //Set the offline scene to null
        manager.offlineScene = "";

        //Make the active scene the offline scene
        SceneManager.LoadScene("MainMenu");

        //Leave Steam Lobby

        if (isOwned)
        {
            if (isServer)
            {
                if (manager.GamePlayers.Count > 1)
                {
                    foreach (PlayerObjectController player in manager.GamePlayers)
                    {
                        if (player != this)
                        {
                            SteamLobby.Instance.ChangeHost((CSteamID)SteamLobby.Instance.CurrentLobbyID, (CSteamID)player.PlayerSteamID);
                        }
                    }
                }
                
                manager.StopHost();
            }
            else
            {
                manager.StopClient();
            }
        }

        SteamLobby.Instance.LeaveLobby();
    }
}
