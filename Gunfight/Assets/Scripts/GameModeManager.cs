using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;
using UnityEngine.Rendering.UI;
using UnityEngine.SceneManagement;

public class GameModeManager : NetworkBehaviour
{
    public static GameModeManager Instance;

    [SyncVar]
    private int currentRound = 0; // keeps track of the current round


    public int totalRounds = 3; // keeps track of total amount of rounds

    [SyncVar(hook = nameof(CheckWinCondition))]
    public int aliveNum; // get this from lobby

    private CustomNetworkManager manager;

    public enum GameMode
    {
        FreeForAll = 0,
        Gunfight = 1
    }

    public GameMode gameMode; // get this from lobby

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
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        if (isServer & (SceneManager.GetActiveScene().name != "Lobby"))
        {
            StartRound(); // starts the first round after Awake
        }
    } 

    public void StartRound()
    {
        // setup for round
        currentRound++; // increase round count
        Debug.Log("Round started: " + currentRound);
    }

    public void EndRound()
    {
        if (currentRound < totalRounds) // if current round is less than total rounds
        {
            DeleteWeaponsInGame();
            if (isServer)
                RpcResetGame();
            SpawnWeaponsInGame();
            StartRound();
            // TODO: Reset Map (pots / boxes)
        }
        else // if the current round equals the total round
        {

        }
    }

    public void PlayerDied(PlayerController player)
    {
       aliveNum--;
    }

    void CheckWinCondition(int oldAliveNum, int newAliveNum)
    {
        if (SceneManager.GetActiveScene().name != "Lobby")
        {
            // If only one player is alive, end round 
            if (aliveNum <= 1)
            {
                EndRound();
                aliveNum = manager.GamePlayers.Count;
            }
        }
    }

    [ClientRpc]
    private void RpcResetGame()
    {
        // Call the reset function for all players
        foreach (PlayerObjectController player in Manager.GamePlayers)
        {
            player.GetComponent<PlayerController>().Respawn();
        }
    }

    public void SpawnWeaponsInGame()
    {
        // Find the WeaponSpawning script in the "game" scene
        WeaponSpawning weaponSpawning = FindObjectOfType<WeaponSpawning>();

        if (weaponSpawning != null)
        {
            // Call the SpawnWeapons method
            weaponSpawning.SpawnWeapons();
        }
        else
        {
            Debug.LogError("WeaponSpawning script not found in the 'game' scene.");
        }
    }

    // This method is called when you want to delete weapons in the "game" scene
    public void DeleteWeaponsInGame()
    {
        // Find the WeaponSpawning script in the "game" scene
        WeaponSpawning weaponSpawning = FindObjectOfType<WeaponSpawning>();

        if (weaponSpawning != null)
        {
            // Call the DeleteWeapons method
            weaponSpawning.DeleteWeapons();
        }
        else
        {
            Debug.LogError("WeaponSpawning script not found in the 'game' scene.");
        }
    }

    public void ShowWinner(string winner)
    {
        // Find the WeaponSpawning script in the "game" scene
        GameModeUIController gameModeUIController = FindObjectOfType<GameModeUIController>();

        if (gameModeUIController != null)
        {
            // Call the DeleteWeapons method
            gameModeUIController.DisplayWinner(winner);
        }
    }
}
