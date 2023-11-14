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
    public int currentRound = 0; // keeps track of the current round


    public int totalRounds = 3; // keeps track of total amount of rounds

    [SyncVar(hook = nameof(CheckWinCondition))]
    public int aliveNum; // get this from lobby

    private CustomNetworkManager manager;

    private int playerCount;

    private bool hasGameStarted = false;

    public enum GameMode
    {
        FreeForAll = 0,
        Gunfight = 1,
        SinglePlayer = 2
    }

    public GameMode gameMode; // get this from lobby

    [Tooltip("Below are used for Single Player")]
    public GameObject enemyPrefab;
    public int startingNumberOfEnemies = 4;
    public int enemyMultiplier = 2;
    public int currentRoundNumberOfEnemies;
    public int currentNumberOfEnemies;

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
    }

    private void Update()
    {
        if (!isServer)
        {
            return;
        }
        if (!hasGameStarted && (SceneManager.GetActiveScene().name != "Lobby") && aliveNum != 0)
        {
            if (gameMode == GameMode.SinglePlayer)
            {
                initEnemy();
            }

            if (isServer)
            {
                playerCount = aliveNum;
                hasGameStarted = true;
                StartRound(); // starts the first round after Awake
            }
        }
    }

    private void initEnemy()
    {
        if (!isServer)
        {
            return;
        }
        for (int i = 0; i < startingNumberOfEnemies; i++)
        {
            float x = (i % 2 == 0) ? 18 : -18;
            float y = (i < 2) ? 22 : -22;

            Vector3 spawnPos = new Vector3(x, y, 0);

            GameObject enemyInstance = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            NetworkServer.Spawn(enemyInstance);
        }
    }

    public void StartRound()
    {
        if (!isServer)
        {
            return;
        }
        // setup for round
        currentRound++; // increase round count
        Debug.Log("Round started: " + currentRound);
    }

    public void EndRound()
    {
        if (!isServer)
        {
            return;
        }
        if (currentRound < totalRounds) // if current round is less than total rounds
        {
            DeleteWeaponsInGame();
            if (isServer)
                RpcResetGame();
            SpawnWeaponsInGame();
            aliveNum = playerCount;
            StartRound();
            // TODO: Reset Map (pots / boxes)
        }
        else // if the current round equals the total round
        {
            //DisplayOverallWinner();
            //GoToLobby();
        }
    }

    public void PlayerDied(PlayerController player)
    {
        player.poc.isAlive = false;
        aliveNum--;
    }

    private IEnumerator DelayedEndRound()
    {
        if (isServer && SceneManager.GetActiveScene().name != "Lobby" && aliveNum != playerCount)
        {
            // If only one player is alive, end round 
            if (aliveNum <= 1)
            {
                RpcShowWinner("Winner: " + FindWinner());
                StartCoroutine(Countdown());
                yield return new WaitForSeconds(5f);
                RpcStopShowWinner();
                EndRound();
            }
        }
    }

    // Coroutine to handle the countdown visualization
    private IEnumerator Countdown()
    {
        float countdownTime = 5f;

        while (countdownTime > 0)
        {
            // Update the countdown text on the UI
            RpcShowCount(Mathf.Ceil(countdownTime).ToString());

            // Wait for the next frame
            yield return null;

            // Reduce the countdown time
            countdownTime -= Time.deltaTime;
        }

        // Clear the countdown text when the countdown is complete
        RpcStopShowCount();
    }

    private string FindWinner()
    {
        foreach (PlayerObjectController player in Manager.GamePlayers)
        {
            if(player.isAlive)
            {
                return player.PlayerName;
            }
        }
        return "No one";
    }

    void CheckWinCondition(int oldAliveNum, int newAliveNum)
    {       
        StartCoroutine(DelayedEndRound());
    }

    [ClientRpc]
    private void RpcResetGame()
    {
        // Call the reset function for all players
        foreach (PlayerObjectController player in Manager.GamePlayers)
        {
            player.GetComponent<PlayerController>().Respawn();
            player.isAlive = true;
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

    [ClientRpc]
    public void RpcShowWinner(string winner)
    {
        GameModeUIController gameModeUIController = FindObjectOfType<GameModeUIController>();

        if (gameModeUIController != null)
        {
            gameModeUIController.DisplayWinner(winner);
        }
    }

    [ClientRpc]
    public void RpcStopShowWinner()
    {
        GameModeUIController gameModeUIController = FindObjectOfType<GameModeUIController>();

        if (gameModeUIController != null)
        {
            gameModeUIController.StopDisplayWinner();
        }
    }

    [ClientRpc]
    public void RpcShowCount(string count)
    {
        GameModeUIController gameModeUIController = FindObjectOfType<GameModeUIController>();

        if (gameModeUIController != null)
        {
            gameModeUIController.DisplayCount(count);
        }
    }

    [ClientRpc]
    public void RpcStopShowCount()
    {
        GameModeUIController gameModeUIController = FindObjectOfType<GameModeUIController>();

        if (gameModeUIController != null)
        {
            gameModeUIController.StopDisplayCount();
        }
    }
}
