using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class GameModeManager : NetworkBehaviour
{
    public static GameModeManager instance;

    // List to keep track of all the players in the game
    private List<PlayerMovementController> players = new List<PlayerMovementController>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void AddPlayer(PlayerMovementController player)
    {
        players.Add(player);
    }

    public void RemovePlayer(PlayerMovementController player)
    {
        players.Remove(player);
    }

    [Server]
    public void PlayerDied(PlayerMovementController player)
    {
        player.alive = false;
        CheckWinCondition();
    }

    [Server]
    private void CheckWinCondition()
    {
        int alivePlayers = 0;
        PlayerMovementController lastAlivePlayer = null;

        // Count alive players and find the last alive player
        foreach (PlayerMovementController player in players)
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
            RpcResetGame();
        }
    }

    [ClientRpc]
    private void RpcResetGame()
    {
        Debug.Log("Reseting");
        // Call the reset function for all players
        foreach (PlayerMovementController player in players)
        {
            Debug.Log(player.name);
            player.RpcRespawn();

            // You can add other reset logic specific to your game here
        }
    }
}
