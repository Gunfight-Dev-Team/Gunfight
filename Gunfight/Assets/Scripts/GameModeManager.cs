using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;
using UnityEngine.Rendering.UI;

public class GameModeManager : NetworkBehaviour
{
    public static GameModeManager instance;

    [SyncVar]
    private float countdownTimer;

    public TextMeshProUGUI countdownText;

    //private bool activeRound = false;

    [SyncVar]
    private int currentRound = 1;
    private int totalRounds = 2;

    // List to keep track of all the players in the game
    private List<PlayerController> players = new List<PlayerController>();

    public enum GameMode
    {
        FreeForAll = 0,
        Gunfight = 1
    }

    public GameMode gameMode;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            StartRound();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    // [Server]
    public void StartRound()
    {
        // setup for round
        Debug.Log("Round: " + currentRound);
        //activeRound = true;
        countdownTimer = 3;
        countdownText.gameObject.SetActive(true);
        StartCoroutine(StartRoundCountdown());
        RpcStartRound();
    }

    [Server]
    public void EndRound()
    {
        if (currentRound < totalRounds) // still more rounds to go
        {
            Debug.Log("End of round");
            for(int i = 0; i < 5; i++){}
            currentRound++;
            StartRound();
        }
        else // ended final round
        {
            Debug.Log("End of game");
        }

        Debug.Log("winner: ");
    }

    [Server]
    public void RoundCompleted()
    {
        RpcEndRound();
        RpcResetGame();
        EndRound();
    }

    private IEnumerator StartRoundCountdown()
    {
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

        //activeRound = true;
        //RpcStartRound();
    }

    [ClientRpc]
    private void RpcStartRound()
    {
        // start the round on all clients
        StartCoroutine(StartRoundCountdown());
    }

    [ClientRpc]
    private void RpcEndRound()
    {
        // end round on all clients
    }

    public void AddPlayer(PlayerController player)
    {
        players.Add(player);
    }

    public void RemovePlayer(PlayerController player)
    {
        players.Remove(player);
    }

    [Server]
    public void PlayerDied(PlayerController player)
    {
        player.alive = false;
        CheckWinCondition();
    }

    [Server]
    private void CheckWinCondition()
    {
        int alivePlayers = 0;
        PlayerController lastAlivePlayer = null;

        // Count alive players and find the last alive player
        foreach (PlayerController player in players)
        {
            if (player.alive)
            {
                alivePlayers++;
                lastAlivePlayer = player;
            }
        }

        // If only one player is alive, call the reset function for all players
        if (alivePlayers <= 1)
        {
            //activeRound = false;
            countdownText.gameObject.SetActive(true);
            // yield return new WaitForSeconds(5);
            countdownText.text = "Round over";
            RoundCompleted();
            //RpcResetGame();
        }
    }

    [ClientRpc]
    private void RpcResetGame()
    {
        Debug.Log("Resetting");
        // Call the reset function for all players
        foreach (PlayerController player in players)
        {
            Debug.Log(player.name);
            player.RpcRespawn();

            // You can add other reset logic specific to your game here
        }
    }
}
