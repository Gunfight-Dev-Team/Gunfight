using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FreeForAllMode : CompetitiveGameMode
{
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

    public override void InitializeGameMode()
    {
        mapManager = GameObject.Find("MapManager").GetComponent<MapManager>();

        playerCount = aliveNum;
        hasGameStarted = true;
        StartRound();
    }

    public override void ResetOverallGame()
    {
        RpcResetPlayerStats();
        RpcResetGame();
    }

    public override bool CheckRoundWinCondition()
    {
        return aliveNum <= 1;
    }

    public override string FindWinner()
    {
        foreach (PlayerObjectController player in Manager.GamePlayers)
        {
            if (player.isAlive)
            {
                player.wins++;
                return player.PlayerName;
            }
        }
        return "No one";
    }

    public override string FindOverallWinner()
    {
        foreach (PlayerObjectController player in Manager.GamePlayers)
        {
            if (player.wins == totalRounds)
            {
                return player.PlayerName;
            }
        }
        return "No one";
    }

    public override bool CheckOverallWin()
    {
        foreach (PlayerObjectController player in Manager.GamePlayers)
        {
            // checks if a player has the required amount of wins
            if (player.wins == totalRounds)
            {
                return true;
            }
        }
        return false;
    }

    public override void RankingList()
    {
        string rankingString = "";
        string winsString = "";

        List<PlayerObjectController> players = new List<PlayerObjectController>();
        foreach (PlayerObjectController player in Manager.GamePlayers)
        {
            players.Add(player);
        }

        players = players.OrderByDescending(player => player.wins).ToList();

        // creates strings with the values from the list
        for (int i = 0; i < playerCount; i++)
        {
            rankingString += players[i].PlayerName + "\n";
            winsString += players[i].wins + "\n";
        }

        Debug.Log("Ranking names: " + rankingString);
        Debug.Log("Ranking wins: " + winsString);

        gameModeUIController.RpcShowRanking(rankingString, winsString);
    }

    public override void PlayerQuit()
    {
        aliveNum--;
        playerCount--;

        Debug.Log("player left the game");

        if (playerCount == 1)
        {
            Debug.Log("One player left");

            // reset stats
            RpcResetPlayerStats();
            currentRound = 0;
            GameModeManager.Instance.playersQuit = true;
        }
    }

    public override void StatsList()
    {
        foreach (PlayerObjectController player in Manager.GamePlayers)
        {
            if (!PlayerStatsItems.Any(b=>b.ConnectionID == player.ConnectionID))
            {
                GameObject NewPlayerStatsItem = Instantiate(PlayerStatsItemPrefab) as GameObject;
                PlayerStatsItem NewStatsItemScript = NewPlayerStatsItem.GetComponent<PlayerStatsItem>();
                NewStatsItemScript.ConnectionID = player.ConnectionID;
                NewStatsItemScript.PlayerSteamID = player.PlayerSteamID;
                
                NewStatsItemScript.SetPlayerStats(player.wins);

                GameObject canvas = GameObject.Find("Canvas");
                // gets the Teams object in the RoundStats object
                GameObject statsList = canvas.transform.GetChild(6).GetChild(0).GetChild(1).gameObject;

                if (statsList == null)
                {
                    Debug.Log("teams object not found");
                }
                else
                {
                    Debug.Log("teams object found");
                }

                NewPlayerStatsItem.transform.SetParent(statsList.transform);

                PlayerStatsItems.Add(NewStatsItemScript);
            }
            else
            {
                foreach(PlayerStatsItem PlayerStatsItemScript in PlayerStatsItems)
                {
                    if (PlayerStatsItemScript.ConnectionID == player.ConnectionID)
                    {
                        PlayerStatsItemScript.SetPlayerStats(player.wins);
                    }
                }
            }
        }
    }
}
