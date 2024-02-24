using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using static GameModeManager;
using UnityEngine.SceneManagement;

public abstract class CompetitiveGameMode : NetworkBehaviour, IGameMode
{
    public static GameModeManager Instance;
    public MapManager mapManager;
    public CardManager cardManager;

    [SyncVar]
    public int currentRound = 0; // keeps track of the current round
    public int totalRounds = 3; // keeps track of total amount of rounds

    [SyncVar(hook = nameof(CheckWinCondition))]
    public int aliveNum; // get this from lobby

    // had to make protected so child classes can use it
    protected CustomNetworkManager manager;

    // also changed to protected during refactor
    protected int playerCount;
    private bool hasGameStarted = false;

    // keeps track of the rankings
    public List<string> ranking = new List<string>();

    public bool quitClicked = false;

    [Header("Card Attributes")]
    private int winningCard;
    public bool useCards = true;


    public abstract string FindWinner();
    public abstract string FindOverallWinner();
    public abstract bool CheckOverallWin();
    public abstract void RankingList();
    public abstract void ResetOverallGame();
    public abstract bool CheckRoundWinCondition();

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

    public void StartRound()
    {
        if (!isServer)
        {
            return;
        }
        // setup for round
        RpcResetGame();
        currentRound++; // increase round count
        Debug.Log("Round started: " + currentRound);
    }

    public void EndRound()
    {
        if (!isServer)
        {
            return;
        }
        if (!CheckOverallWin()) // if there is not an overall winner
        {
            DeleteWeaponsInGame();
            if (isServer)
                RpcResetGame();
            SpawnWeaponsInGame();
            aliveNum = playerCount;
            StartRound();
            // TODO: Reset Map (pots / boxes)
        }
        else // if there is an overall winner
        {
            Debug.Log("End of game!");
            GameModeUIController gameModeUIController = FindObjectOfType<GameModeUIController>();
            gameModeUIController.DisplayQuitButton();
            RpcShowRoundPanel();
            RankingList();
            RpcShowWinner("Overall Winner: " + FindOverallWinner());

            //reset player stats
            ResetOverallGame();

            currentRound = 0;

            StartCoroutine(QuitCountdown());
        }
    }

    public IEnumerator DelayedEndRound()
    {
        if (isServer && SceneManager.GetActiveScene().name != "Lobby" && aliveNum != playerCount)
        {
            if (useCards)
            {
                // gets the Card Manager game object
                if (cardManager == null)
                {
                    cardManager = FindObjectOfType<CardManager>();
                    if (cardManager == null)
                    {
                        Debug.Log("Couldnt find card manager (DelayedEndRound)");
                    }
                }

                Debug.Log("Found card manager: " + (cardManager != null));
            }

            // If only one player is alive or there is a team winner, end round 
            if (CheckRoundWinCondition())
            {
                RpcDisableGameInteraction();
                string winner = FindWinner();
                if (!CheckOverallWin())
                {
                    if (useCards)
                    {
                        cardManager.RpcShowCardPanel();
                    }

                    RpcShowRoundPanel();
                    RpcShowWinner("Winner: " + winner);
                    RpcShowRoundNumber("Round: " + Mathf.Ceil(currentRound).ToString());
                    RankingList(); // displays the rankings

                    if (useCards)
                    {
                        // start 10s timer 
                        int count = 10;

                        while (count > 0)
                        {
                            RpcShowTimer(Mathf.Ceil(count).ToString());
                            // if everyone voted stop countdown
                            if (cardManager.CheckIfEveryoneVoted(playerCount))
                            {
                                Debug.Log("Break countdown");
                                break;
                            }
                            yield return new WaitForSeconds(1f);
                            count--;
                            Debug.Log("Card countdown: " + count);
                        }

                        RpcStopShowTimer();

                        // find the card voted the most
                        winningCard = cardManager.FindMaxVote();
                        Debug.Log("Winning card: " + winningCard);

                        cardManager.RpcShowWinningCard(winningCard); // only displaying the winning card
                        yield return new WaitForSeconds(5f); // pause to show winning card
                    }
                    else // if cards are not being used
                    {
                        yield return new WaitForSeconds(5f);
                    }

                    RpcStopShowRanking();
                    RpcStopShowRoundNumber();
                    RpcStopShowWinner();

                    if (useCards)
                    {
                        cardManager.RpcStopCardPanel();
                    }

                    RpcStopShowRoundPanel();
                    StartCoroutine(PreroundCountdown());
                    yield return new WaitForSeconds(5f);
                }
                EndRound();
            }
        }
    }

    public IEnumerator PreroundCountdown()
    {
        float countdownTime = 5f;

        while (countdownTime > 0)
        {
            // Update the countdown text on the UI
            RpcShowCount(Mathf.Ceil(countdownTime).ToString());

            // Wait for the next frame
            yield return null;
            countdownTime -= Time.deltaTime;
        }

        RpcStopShowCount();
    }

    public void ToLobby()
    {
        manager.StartGame("Lobby");
    }

    public IEnumerator QuitCountdown()
    {
        // 10s countdown 
        int count = 10;
        while (count > 0)
        {
            if (quitClicked)
            {
                break;
            }
            yield return new WaitForSeconds(1f);
            count--;
        }
        Debug.Log("Quit game");
        ToLobby();
    }

    public void PlayerDied(PlayerController player)
    {
        player.poc.isAlive = false;
        aliveNum--;
    }

    public void CheckWinCondition(int oldAliveNum, int newAliveNum)
    {
        StartCoroutine(DelayedEndRound());
    }

    public void SpawnWeaponsInGame()
    {
        // Find the WeaponSpawning script in the "game" scene
        WeaponSpawning weaponSpawning = FindObjectOfType<WeaponSpawning>();

        if (weaponSpawning != null)
        {
            weaponSpawning.SpawnWeapons();
        }
        else
        {
            Debug.LogError("WeaponSpawning script not found in the 'game' scene.");
        }
    }

    public void DeleteWeaponsInGame()
    {
        // Find the WeaponSpawning script in the "game" scene
        WeaponSpawning weaponSpawning = FindObjectOfType<WeaponSpawning>();

        if (weaponSpawning != null)
        {
            weaponSpawning.DeleteWeapons();
        }
        else
        {
            Debug.LogError("WeaponSpawning script not found in the 'game' scene.");
        }
    }

    //---------------------------------------------------------------------------
    //--------------------temporary UI RPC Methods-------------------------------
    //---------------------------------------------------------------------------

    [ClientRpc]
    public void RpcShowRoundPanel()
    {
        GameModeUIController gameModeUIController = FindObjectOfType<GameModeUIController>();

        if (gameModeUIController != null)
        {
            gameModeUIController.DisplayRoundPanel();
        }
    }

    [ClientRpc]
    public void RpcStopShowRoundPanel()
    {
        GameModeUIController gameModeUIController = FindObjectOfType<GameModeUIController>();

        if (gameModeUIController != null)
        {
            gameModeUIController.StopDisplayRoundPanel();
        }
    }

    [ClientRpc]
    public void RpcShowRanking(string rankings, string wins)
    {
        GameModeUIController gameModeUIController = FindObjectOfType<GameModeUIController>();

        if (gameModeUIController != null)
        {
            gameModeUIController.DisplayRanking(rankings, wins);
        }
    }

    [ClientRpc]
    public void RpcStopShowRanking()
    {
        GameModeUIController gameModeUIController = FindObjectOfType<GameModeUIController>();

        if (gameModeUIController != null)
        {
            gameModeUIController.StopDisplayRanking();
        }
    }

    [ClientRpc]
    public void RpcDisableGameInteraction()
    {
        // Call the disable game interaction for all players
        foreach (PlayerObjectController player in Manager.GamePlayers)
        {
            player.GetComponent<PlayerController>().enabled = false;
        }
    }

    [ClientRpc]
    public void RpcResetGame()
    {
        // Call the reset function for all players
        foreach (PlayerObjectController player in Manager.GamePlayers)
        {
            player.GetComponent<PlayerController>().enabled = true;
            player.GetComponent<PlayerController>().Respawn();
            player.isAlive = true;
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

    [ClientRpc]
    public void RpcShowTimer(string count)
    {
        GameModeUIController gameModeUIController = FindObjectOfType<GameModeUIController>();

        if (gameModeUIController != null)
        {
            gameModeUIController.DisplayTimer(count);
        }
    }

    [ClientRpc]
    public void RpcStopShowTimer()
    {
        GameModeUIController gameModeUIController = FindObjectOfType<GameModeUIController>();

        if (gameModeUIController != null)
        {
            gameModeUIController.StopDisplayTimer();
        }
    }

    [ClientRpc]
    public void RpcShowRoundNumber(string number)
    {
        GameModeUIController gameModeUIController = FindObjectOfType<GameModeUIController>();

        if (gameModeUIController != null)
        {
            gameModeUIController.DisplayRoundNumber(number);
        }
    }

    [ClientRpc]
    public void RpcStopShowRoundNumber()
    {
        GameModeUIController gameModeUIController = FindObjectOfType<GameModeUIController>();

        if (gameModeUIController != null)
        {
            gameModeUIController.StopDisplayRoundNumber();
        }
    }

    [ClientRpc]
    public void RpcResetPlayerStats()
    {
        // reset wins for all players
        foreach (PlayerObjectController player in Manager.GamePlayers)
        {
            player.wins = 0;
        }
    }
}
