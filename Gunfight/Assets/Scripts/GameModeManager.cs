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

    // get this from lobby
    private int totalRounds = 3; // keeps track of total amount of rounds

    [SyncVar(hook = nameof(CheckWinCondition))]
    public int aliveNum = 0; // get this from lobby

    private CustomNetworkManager manager;

    public enum GameMode
    {
        FreeForAll = 0,
        Gunfight = 1
    }

    public GameMode gameMode; // get this from lobby

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
        if (isServer & (SceneManager.GetActiveScene().name != "Lobby"))
        {
            StartRound(); // starts the first round after Awake
        }
    } 

    public void StartRound()
    {
        // setup for round
        Debug.Log("Round started: " + currentRound);
    }

    public void EndRound()
    {
        if(isServer)
        {
            if (currentRound < totalRounds) // if current round is less than total rounds
            {
                currentRound++; // increase round count
                //yield return WaitForSeconds(5);
                RpcResetGame();
                StartRound();
            }
            else // if the current round equals the total round
            {

            }
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
            }
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
            player.GetComponent<PlayerController>().RpcRespawn();
        }
    }
}
