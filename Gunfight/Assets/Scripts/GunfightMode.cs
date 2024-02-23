using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunfightMode : IGameMode
{
    public int[] teamAlive = { 0, 0 }; // keeps track of how many players on each team is alive
    public int[] teamWins = { 0, 0 }; // keeps track of how many wins each team has
    private bool teamWinner = false;
    private int teamWinNum;

    public bool quitClicked = false; // keeps track if quit button was clicked

    private void GetTeamPlayers()
    {
        //assigns the number of players on each team
        foreach (PlayerObjectController player in Manager.GamePlayers)
        {
            if (player.Team == 1)
            {
                teamAlive[0]++;
            }
            else if (player.Team == 2)
            {
                teamAlive[1]++;
            }
        }

        Debug.Log("Team 1 players alive: " + teamAlive[0]);
        Debug.Log("Team 2 players alive: " + teamAlive[1]);
    }

    private bool CheckTeamWin()
    {
        int teamOneAlive = 0;
        int teamTwoALive = 0;

        foreach (PlayerObjectController player in Manager.GamePlayers)
        {
            if (player.isAlive)
            {
                if (player.Team == 1)
                {
                    teamOneAlive++;
                }
                else if (player.Team == 2)
                {
                    teamTwoALive++;
                }
            }
        }

        if (teamOneAlive == 0 || teamTwoALive == 0)
        {
            if (teamOneAlive == 0)
            {
                teamWinNum = 2;
            }
            else if (teamTwoALive == 0)
            {
                teamWinNum = 1;
            }
            return true;
        }
        return false;
    }
}
