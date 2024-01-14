using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Steamworks;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.SceneManagement;
using TMPro;
using System;
using Unity.VisualScripting;

public class LobbyController : MonoBehaviour
{
    public static LobbyController Instance;
    public GameModeManager gameModeManager;

    public InputField LobbyNameInput;

    public GameObject PlayerListViewContent;
    public GameObject PlayerList2ViewContent;
    public GameObject PlayerListItemPrefab;
    public GameObject LocalPlayerObject;
    public GameObject PlayerList2;
    public GameObject ChatBox;

    public ulong CurrentLobbyID;
    public bool PlayerItemCreated = false;
    private List<PlayerListItem> PlayerListItems = new List<PlayerListItem>();
    public PlayerObjectController LocalPlayerController;

    public Button ReadyButton;
    public Toggle publicToggle;
    public TMP_Text ReadyButtonText;
    public Dropdown GameModeChooser;
    public Dropdown RoundNumChooser;

    public bool isPublic = false;
    public bool AllReady = false;

    private CustomNetworkManager manager;

    //Maps
    public string MapName = "Game";

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

    private void Awake()
    {
        if(Instance == null) { Instance = this; }
    }

    private void OnEnable()
    {
        if (GameObject.Find("LocalGamePlayer") != null)
        {
            Instance.FindLocalPlayer();
        }
        Instance.UpdateLobbyName();
        Instance.UpdatePlayerList();
    }

    private void Start()
    {
        if (LocalPlayerController == null)
        {
            LocalPlayerController = GameObject.Find("LocalGamePlayer").GetComponent<PlayerObjectController>();
        }
        if (GameObject.Find(gameModeManager.name) == null)
        {
            gameModeManager = Instantiate(gameModeManager);
            if (LocalPlayerController.PlayerIdNumber == 1)
            {
                NetworkServer.Spawn(gameModeManager.GameObject());
            }
        }

        if (LocalPlayerController.PlayerIdNumber == 1)
        {
            // if host
            // able to control game access
            publicToggle.interactable = true;
            // able to edit lobby name
            LobbyNameInput.interactable = true;
            LobbyNameInput.onEndEdit.AddListener(OnEndEdit);
            // able to choose gmae mode
            GameModeChooser.interactable = true;
            //able to choose number of rounds
            RoundNumChooser.interactable = true;
            // able to change map, this is handled in mapController
            
        }
        else 
        {
            publicToggle.interactable = false;
            GameModeChooser.interactable = false;
            RoundNumChooser.interactable = false;
            LobbyNameInput.interactable = false;
        }


        if(PlayerPrefs.GetInt("isJoinedSingle") == 1)
        {
            // Joined single player
            GameModeChooser.value = 2;
            GameModeManager.Instance.gameMode = GameModeManager.GameMode.SinglePlayer;
        }
    }

    private void OnEndEdit(string newName)
    {
        // Ensure the lobby name is not empty
        if (!string.IsNullOrEmpty(newName))
        {
            // Update the lobby name using Steamworks
            SteamMatchmaking.SetLobbyData((CSteamID)CurrentLobbyID, "name", newName);
        }
    }

    public void PrepareToStartGame()
    {
        if(LocalPlayerController.PlayerIdNumber == 1 && AllReady)
        {
            // if host, should be able to start the game
            StartGame();
        }
        else
        {
            ReadyPlayer();
        }
    }

    public void ReadyPlayer()
    {
        LocalPlayerController.ChangeReady();
    }

    public void ChangeToTeamOne()
    {
        LocalPlayerController.ChangeTeam(1);
    }

    public void ChangeToTeamTwo()
    {
        LocalPlayerController.ChangeTeam(2);
    }

    public void UpdateButton()
    {
        if(LocalPlayerController.Ready && LocalPlayerController.PlayerIdNumber != 1)
        {
            ReadyButtonText.text = "Ready";
        }
        else if(AllReady && LocalPlayerController.PlayerIdNumber == 1)
        {
            ReadyButtonText.text = "Start";
            ReadyButton.interactable = true;
        }
        else if (!LocalPlayerController.Ready && LocalPlayerController.PlayerIdNumber != 1)
        {
            ReadyButtonText.text = "Unready";
        }
    }


    public void CheckIfAllReady()
    {
        bool isReady = true;
        foreach(PlayerObjectController player in Manager.GamePlayers)
        {
            if(player.PlayerIdNumber != 1)
            {
                if (player.Ready)
                {
                    isReady = true;
                }
                else
                {
                    isReady = false;
                    break;
                }
            }
        }

        if (isReady)
        {
            AllReady = true;
        }
        else
        {
            AllReady = false;
            if(LocalPlayerController.PlayerIdNumber == 1)
            {
                ReadyButton.interactable = false;
            }
        }
    }

    public void UpdateLobbyName()
    {
        CurrentLobbyID = Manager.GetComponent<SteamLobby>().CurrentLobbyID;
        LobbyNameInput.text = SteamMatchmaking.GetLobbyData(new CSteamID(CurrentLobbyID), "name");
    }

    public void UpdatePlayerList()
    {
        if(!PlayerItemCreated) { CreateHostPlayerItem(); }
        if(PlayerListItems.Count < Manager.GamePlayers.Count) { CreateClientPlayerItem(); }
        if(PlayerListItems.Count > Manager.GamePlayers.Count) { RemovePlayerItem(); }
        if(PlayerListItems.Count == Manager.GamePlayers.Count) { UpdatePlayerItem(); }
        SyncLobbyData();
    }

    public void SyncLobbyData()
    {
        if (this == null)
            return; // gets rid of error when shutting down the game and lobby controller has already been deleted.
        // Sync Gamemode Text
        GetComponent<GameModeDropdown>().OnDropdownValueChanged();
        SwitchGameModes();
    }

    public void FindLocalPlayer()
    {
        LocalPlayerObject = GameObject.Find("LocalGamePlayer");
        LocalPlayerController = LocalPlayerObject.GetComponent<PlayerObjectController>();
    }

    public void CreateHostPlayerItem()
    {
        foreach(PlayerObjectController player in Manager.GamePlayers)
        {
            GameObject NewPlayerItem = Instantiate(PlayerListItemPrefab) as GameObject;
            PlayerListItem NewPlayerItemScript = NewPlayerItem.GetComponent<PlayerListItem>();

            NewPlayerItemScript.PlayerName = player.PlayerName;
            NewPlayerItemScript.ConnectionID = player.ConnectionID;
            NewPlayerItemScript.PlayerSteamID = player.PlayerSteamID;
            NewPlayerItemScript.Ready = player.Ready;
            NewPlayerItemScript.Team = player.Team;

            NewPlayerItemScript.SetPlayerValues();

            if (player.Team == 2)
            {
                NewPlayerItem.transform.SetParent(PlayerList2ViewContent.transform);
            }
            else
            {
                NewPlayerItem.transform.SetParent(PlayerListViewContent.transform);
            }
            NewPlayerItem.transform.localScale = Vector3.one;

            PlayerListItems.Add(NewPlayerItemScript);
        }
        PlayerItemCreated = true;
    }

    public void CreateClientPlayerItem()
    {
        foreach (PlayerObjectController player in Manager.GamePlayers)
        {
            if(!PlayerListItems.Any(b=> b.ConnectionID == player.ConnectionID))
            {
                GameObject NewPlayerItem = Instantiate(PlayerListItemPrefab) as GameObject;
                PlayerListItem NewPlayerItemScript = NewPlayerItem.GetComponent<PlayerListItem>();

                NewPlayerItemScript.PlayerName = player.PlayerName;
                NewPlayerItemScript.ConnectionID = player.ConnectionID;
                NewPlayerItemScript.PlayerSteamID = player.PlayerSteamID;
                NewPlayerItemScript.Ready = player.Ready;
                NewPlayerItemScript.Team = player.Team;

                NewPlayerItemScript.SetPlayerValues();

                if (player.Team == 2)
                {
                    NewPlayerItem.transform.SetParent(PlayerList2ViewContent.transform);
                }
                else
                {
                    NewPlayerItem.transform.SetParent(PlayerListViewContent.transform);
                }
                NewPlayerItem.transform.localScale = Vector3.one;

                PlayerListItems.Add(NewPlayerItemScript);
            }
        }
    }

    public void UpdatePlayerItem()
    {
        foreach (PlayerObjectController player in Manager.GamePlayers)
        {
            foreach(PlayerListItem PlayerListItemScript in PlayerListItems)
            {
                if(PlayerListItemScript.ConnectionID == player.ConnectionID)
                {
                    PlayerListItemScript.PlayerName = player.PlayerName;
                    PlayerListItemScript.Ready = player.Ready;
                    PlayerListItemScript.Team = player.Team;
                    PlayerListItemScript.SetPlayerValues();

                    if (player.Team == 2)
                    {
                        PlayerListItemScript.transform.SetParent(PlayerList2ViewContent.transform);
                    }
                    else
                    {
                        PlayerListItemScript.transform.SetParent(PlayerListViewContent.transform);
                    }
                    PlayerListItemScript.transform.localScale = Vector3.one;

                    if (player == LocalPlayerController)
                    {
                        UpdateButton();
                    }
                }
            }
        }

        CheckIfAllReady();
    }

    public void RemovePlayerItem()
    {
        List<PlayerListItem> playerListItemToRemove = new List<PlayerListItem>();

        foreach (PlayerListItem playerlistItem in PlayerListItems)
        {
            if(!Manager.GamePlayers.Any(b=> b.ConnectionID == playerlistItem.ConnectionID))
            {
                playerListItemToRemove.Add(playerlistItem);
            }
        }
        if(playerListItemToRemove.Count > 0)
        {
            foreach(PlayerListItem playerlistItemToRemove in playerListItemToRemove)
            {
                if (playerlistItemToRemove != null)
                {
                    GameObject ObjectToRemove = playerlistItemToRemove.gameObject;
                    PlayerListItems.Remove(playerlistItemToRemove);
                    Destroy(ObjectToRemove);
                    ObjectToRemove = null;
                }
            }
        }
    }

    public void StartGame()
    {
        GameModeManager.Instance.aliveNum = manager.GamePlayers.Count;
        LocalPlayerController.CanStartGame(MapName);
    }

    public void TogglePublic()
    {
        if (!isPublic)
        {
            SteamMatchmaking.SetLobbyType(new CSteamID(CurrentLobbyID), ELobbyType.k_ELobbyTypePublic);
        }
        else
        {
            SteamMatchmaking.SetLobbyType(new CSteamID(CurrentLobbyID), ELobbyType.k_ELobbyTypeFriendsOnly);
        }
        isPublic = !isPublic;
    }

    public void SwitchGameModes()
    {
        if(GameModeChooser.value == 0)
        {
            PlayerList2.SetActive(false);
            ChatBox.SetActive(true);
            gameModeManager.gameMode = GameModeManager.GameMode.FreeForAll;
        }
        else if(GameModeChooser.value == 1)
        {
            PlayerList2.SetActive(true);
            ChatBox.SetActive(false);
            gameModeManager.gameMode = GameModeManager.GameMode.Gunfight;
        }
        else if (GameModeChooser.value == 2)
        {
            PlayerList2.SetActive(false);
            ChatBox.SetActive(true);
            gameModeManager.gameMode = GameModeManager.GameMode.SinglePlayer;
        }

    }

    public void SwitchRoundNum()
    {
        string selectedNum = RoundNumChooser.options[RoundNumChooser.value].text;

        if(int.TryParse(selectedNum, out int intVal))
        {
            GameModeManager.Instance.totalRounds = intVal;
        }
    }

    public void Leave()
    {
        LocalPlayerController.Quit();
    }
}
