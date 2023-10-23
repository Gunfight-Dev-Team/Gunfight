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
    private float countdownTimer; // keeps track of the timer

    public TextMeshProUGUI countdownText; // shows UI of timer in scene

    [SyncVar]
    private int currentRound = 0; // keeps track of the current round
    private int totalRounds = 3; // keeps track of total amount of rounds

    private CustomNetworkManager manager;

    public enum GameMode
    {
        FreeForAll = 0,
        Gunfight = 1
    }

    public GameMode gameMode;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

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
        if (isServer & (SceneManager.GetActiveScene().name == "Game"))
        {
            StartRound(); // starts the first round after Awake
        }
    } 

    [Server]
    public void StartRound()
    {
        Debug.Log("Players num: " + Manager.GamePlayers.Count);
        // setup for round
        Debug.Log("Round: " + currentRound);
        countdownTimer = 3; 
        CmdStartRound();
        StartCoroutine(StartRoundCountdown());
    }

    private IEnumerator StartRoundCountdown()
    {
        countdownText.gameObject.SetActive(true);
        // starts the countdown sequence then starts the round
        while (countdownTimer > 0)
        {
            countdownText.text = countdownTimer.ToString();
            Debug.Log("Countdown: " + countdownTimer);
            yield return new WaitForSeconds(1);
            countdownTimer--;
            if (countdownTimer == 0)
            {
                Debug.Log("Round started");
                countdownText.gameObject.SetActive(false);
                break;
            }   
        }
    }

    [Command]
    private void CmdStartRound()
    {
        // start the round on all clients
        RpcStartRound();
    }

    [ClientRpc]
    private void RpcStartRound()
    {
        // start the round on all clients
        Debug.Log("RpcStartRound");
        StartCoroutine(StartRoundCountdown());
    }

    public void RoundCompleted()
    {
        if (isServer)
        {
            CmdEndRound();
            EndRound();
        }
    }

    [Server]
    public void EndRound()
    {
        if (isServer)
        {
            if (currentRound < totalRounds) // still more rounds to go
            {
                Debug.Log("End of round");
                currentRound++;
                RpcResetGame();
                StartRound();
            }
            else if (currentRound > totalRounds)// ended final round
            {
                Debug.Log("End of game");
            }

            Debug.Log("winner: ");
        }
    }

    private IEnumerator EndRoundRoutine()
    {
        countdownText.gameObject.SetActive(true);
        countdownText.text = "Round over";
        yield return new WaitForSeconds(5);
    }

    [Command]
    private void CmdEndRound()
    {
        // start the round on all clients
        RpcEndRound();
    }

    [ClientRpc]
    private void RpcEndRound()
    {
        // end round on all clients
        Debug.Log("RpcEndRound");
        StartCoroutine(EndRoundRoutine());
        // card mechanic handled
    }

    [Server]
    public void PlayerDied(PlayerController player)
    {
        player.alive = false;
        if (isServer)
            CheckWinCondition();
    }

    [Server]
    private void CheckWinCondition()
    {
        int alivePlayers = 0;
        PlayerObjectController lastAlivePlayer = null;

        // Count alive players and find the last alive player
        foreach (PlayerObjectController player in Manager.GamePlayers)
        {
            if (player.GetComponent<PlayerController>().alive)
            {
                alivePlayers++;
                lastAlivePlayer = player;
            }
        }

        // If only one player is alive, call the reset function for all players
        if (alivePlayers <= 1)
        {
            countdownText.gameObject.SetActive(true);
            countdownText.text = "Round over";

            RoundCompleted();
        }
    }

    [ClientRpc]
    private void RpcResetGame()
    {
        Debug.Log("Resetting");
        // Call the reset function for all players
        foreach (PlayerObjectController player in Manager.GamePlayers)
        {
            Debug.Log(player.PlayerName);
            player.GetComponent<PlayerController>().CmdReset();

            // You can add other reset logic specific to your game here
        }
    }
}
