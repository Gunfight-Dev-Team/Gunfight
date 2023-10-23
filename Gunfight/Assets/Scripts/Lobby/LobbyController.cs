using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Steamworks;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.SceneManagement;
using TMPro;

public class LobbyController : MonoBehaviour
{
    public static LobbyController Instance;

    public Text LobbyNameText;
    public InputField LobbyNameInput;

    public GameObject PlayerListViewContent;
    public GameObject PlayerListItemPrefab;
    public GameObject LocalPlayerObject;

    public ulong CurrentLobbyID;
    public bool PlayerItemCreated = false;
    private List<PlayerListItem> PlayerListItems = new List<PlayerListItem>();
    public PlayerObjectController LocalPlayerController;

    public Button ReadyButton;
    public GameObject publicToggle;
    public TMP_Text ReadyButtonText;

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

    private void Start()
    {
        if (LocalPlayerController.PlayerIdNumber == 1)
        {
            publicToggle.SetActive(true);
            // Disable the input field for non-host players
            LobbyNameInput.interactable = true;
            LobbyNameInput.onEndEdit.AddListener(OnEndEdit);
        }
        else 
        {
            LobbyNameInput.interactable = false;
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

        //if (AllReady)
        //{
        //    if(LocalPlayerController.PlayerIdNumber == 1)
        //    {
        //        ReadyButton.interactable = true;
        //        ReadyButtonText.text = "Start";
        //    }
        //    else
        //    {
        //        ReadyButtonText.text = "Unready";
        //    }
        //}
        //else
        //{
        //    ReadyButtonText.text = "Unready";
        //}
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
            NewPlayerItemScript.SetPlayerValues();

            NewPlayerItem.transform.SetParent(PlayerListViewContent.transform);
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
                NewPlayerItemScript.SetPlayerValues();

                NewPlayerItem.transform.SetParent(PlayerListViewContent.transform);
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
                    PlayerListItemScript.SetPlayerValues();
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
                GameObject ObjectToRemove = playerlistItemToRemove.gameObject;
                PlayerListItems.Remove(playerlistItemToRemove);
                Destroy(ObjectToRemove);
                ObjectToRemove = null;
            }
        }
    }

    public void StartGame()
    {
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

    public void Leave()
    {
        LocalPlayerController.Quit();
    }
}
