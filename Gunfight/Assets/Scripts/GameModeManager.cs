using System.Collections;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;

public class GameModeManager : NetworkBehaviour
{
    public static GameModeManager Instance;
    public MapManager mapManager;
    public CardManager cardManager;

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

    [Header("Below are used for Single Player")]
    public GameObject enemyPrefab;
    public int startingNumberOfEnemies = 4;
    public float enemyMultiplier = 1.15f;
    public int currentRoundNumberOfEnemies;
    [SyncVar(hook = nameof(CheckWinConditionSingle))]
    public int currentNumberOfEnemies;

    [Header("Below are used for cards")]
    private int winningCard;
    public bool useCards = false; 

    // keeps track of the rankings
    public List<string> ranking = new List<string>();

    [Header("Gunfight mode")]
    public int[] teamAlive = {0, 0}; // keeps track of how many players on each team is alive
    public int[] teamWins = {0, 0}; // keeps track of how many wins each team has
    private bool teamWinner = false; 
    private int teamWinNum;

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
            mapManager = GameObject.Find("MapManager").GetComponent<MapManager>();
            if (gameMode == GameMode.SinglePlayer)
            {
                totalRounds = 9999;
                initEnemy();
            }

            if (isServer)
            {
                playerCount = aliveNum;
                hasGameStarted = true;
                if (gameMode == GameMode.Gunfight)
                {
                    GetTeamPlayers();
                }
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
        currentNumberOfEnemies = currentRoundNumberOfEnemies;
        for (int i = 0; i < startingNumberOfEnemies; i++)
        {
            float x = (i % 2 == 0) ? mapManager.mapWidth / 2 : -mapManager.mapWidth / 2;
            float y = (i < 2) ? (mapManager.mapHeight - mapManager.heightOffset) / 2 : -(mapManager.mapHeight - mapManager.heightOffset) / 2;

            Vector3 spawnPos = new Vector3(x, y, 0);

            GameObject enemyInstance = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            NetworkServer.Spawn(enemyInstance);
        }
    }

    public void spawnEnemies()
    {
        if (!isServer)
        {
            return;
        }
        for (int i = 0; i < currentRoundNumberOfEnemies; i++)
        {
            float x, y;

            if (i % 2 == 0)
            {
                // Even index, spawn on the top or bottom edge
                x = Random.Range(-mapManager.mapWidth / 2, mapManager.mapWidth / 2);
                y = (i < 2) ? (mapManager.mapHeight - mapManager.heightOffset) / 2 : -(mapManager.mapHeight - mapManager.heightOffset) / 2;
            }
            else
            {
                // Odd index, spawn on the left or right edge
                x = (i < 2) ? mapManager.mapWidth / 2 : -mapManager.mapWidth / 2;
                y = Random.Range(-(mapManager.mapHeight - mapManager.heightOffset) / 2, (mapManager.mapHeight - mapManager.heightOffset) / 2);
            }

            Vector3 spawnPos = new Vector3(x, y, 0);

            GameObject enemyInstance = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            NetworkServer.Spawn(enemyInstance);
        }
        increaseSpeed();
        increaseDamage();
    }

    public void increaseSpeed()
    {
        GameObject[] enemyObjects = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemyObject in enemyObjects)
        {
            // Check if the GameObject has the EnemyObjectController script attached
            EnemyObjectController controller = enemyObject.GetComponent<EnemyObjectController>();

            if (controller != null)
            {
                // Call the updateSpeed function
                controller.updateSpeedExpn(currentRound);
            }
            else
            {
                Debug.LogWarning("EnemyObjectController script not found on GameObject: " + enemyObject.name);
            }
        }
    }

    public void increaseDamage()
    {
        GameObject[] enemyObjects = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemyObject in enemyObjects)
        {
            // Check if the GameObject has the EnemyObjectController script attached
            EnemyObjectController controller = enemyObject.GetComponent<EnemyObjectController>();

            if (controller != null)
            {
                // Call the updateSpeed function
                controller.updateDamageExpn(currentRound);
            }
            else
            {
                Debug.LogWarning("EnemyObjectController script not found on GameObject: " + enemyObject.name);
            }
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

    public void EndRound()
    {
        if (!isServer)
        {
            return;
        }
        if (gameMode != GameMode.SinglePlayer)
        {
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
                RpcShowRoundPanel();
                RankingList();
                RpcShowWinner("Overall Winner: " + FindOverallWinner());
                // need to do a delay before going back to lobby

                // reset players stats
                if (gameMode == GameMode.FreeForAll)
                {
                    RpcResetPlayerStats();
                }
                // reset team stats
                if (gameMode == GameMode.Gunfight)
                {
                    teamWins[0] = 0;
                    teamWins[1] = 0;
                }

                RpcStopShowWinner();
                RpcStopShowRoundPanel();
                currentRound = 0;
                manager.StartGame("Lobby");
            }
        }
        else
        {
            // if single player mode
            DeleteWeaponsInGame();
            if (isServer)
                RpcResetGame();
            SpawnWeaponsInGame();
            currentRoundNumberOfEnemies = Mathf.RoundToInt(currentRoundNumberOfEnemies * enemyMultiplier);
            currentNumberOfEnemies = currentRoundNumberOfEnemies;
            StartRound();
            spawnEnemies();
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
            if (useCards)
            {
                // gets the Card Manager game object
                if (cardManager == null)
                {
                    cardManager = FindObjectOfType<CardManager>();
                    if (cardManager == null)
                    {
                        Debug.Log("Couldnt find game object");
                    }
                }
            
                Debug.Log("Found card manager: " + (cardManager != null));
            }

            if (gameMode == GameMode.Gunfight)
            {
                teamWinner = CheckTeamWin();
            }

            // If only one player is alive or there is a team winner, end round 
            if (aliveNum <= 1 || teamWinner)
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
                    StartCoroutine(Countdown());
                    yield return new WaitForSeconds(5f);
                }
                EndRound();
            }
        }
    }

    private IEnumerator DelayedEndRoundSingle()
    {
        if (isServer && SceneManager.GetActiveScene().name != "Lobby" && 
            currentNumberOfEnemies != startingNumberOfEnemies)
        {
            // gets the Card Manager game object
            if (cardManager == null)
            {
                cardManager = FindObjectOfType<CardManager>();
                if (cardManager == null)
                {
                    Debug.Log("Couldnt find game object");
                }
            }

            // If no enemy, end round 
            if (currentNumberOfEnemies <= 0)
            {
                cardManager.RpcShowCardPanel();
                RpcShowWinner("Round: " + currentRound);
                yield return new WaitForSeconds(10.0f); 
                RpcStopShowWinner();
                cardManager.RpcStopCardPanel();

                StartCoroutine(Countdown());
                yield return new WaitForSeconds(5f);
            }
            EndRound();
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
        if (gameMode == GameMode.FreeForAll)
        {
            foreach (PlayerObjectController player in Manager.GamePlayers)
            {
                if (player.isAlive)
                {
                    player.wins++;
                    return player.PlayerName;
                }
            }
        }

        if (gameMode == GameMode.Gunfight)
        {
            if (teamWinNum == 1)
            {
                teamWins[0]++;
                return "Team 1";
            }
            else if (teamWinNum == 2)
            {
                teamWins[1]++;
                return "Team 2";
            }
        }
        return "No one";
    }

    private string FindOverallWinner()
    {
        if (gameMode == GameMode.FreeForAll)
        {
            foreach (PlayerObjectController player in Manager.GamePlayers)
            {
                if (player.wins == totalRounds)
                {
                    return player.PlayerName;
                }
            }
        }

        if (gameMode == GameMode.Gunfight)
        {
            if (teamWins[0] == totalRounds)
            {
                return "Team 1";
            }
            else if (teamWins[1] == totalRounds)
            {
                return "Team 2";
            }
        }
        return "No one";
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

    private bool CheckOverallWin()
    {
        if (gameMode == GameMode.FreeForAll)
        {
            foreach (PlayerObjectController player in Manager.GamePlayers)
            {
                // checks if a player has the required amount of wins
                if (player.wins == totalRounds)
                {
                    return true;
                }
            }
        }

        if (gameMode == GameMode.Gunfight)
        {
            // check if a team has the required number of wins
            if (teamWins[0] == totalRounds || teamWins[1] == totalRounds)
            {
                Debug.Log("A team has won");
                return true;
            }
        }

        return false;
    }

    void CheckWinCondition(int oldAliveNum, int newAliveNum)
    {
        if (gameMode != GameMode.SinglePlayer)
        {
            StartCoroutine(DelayedEndRound());
        }
    }

    void CheckWinConditionSingle(int oldAliveNum, int newAliveNum)
    {
        StartCoroutine(DelayedEndRoundSingle());
    }

    private void RankingList()
    {
        string rankingString = "";
        string winsString = "";

        if (gameMode == GameMode.FreeForAll)
        {
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
        }

        if (gameMode == GameMode.Gunfight)
        {
            if (teamWinNum == 1)
            {
                rankingString = "Team 1 \nTeam 2\n";
                winsString = teamWins[0] + "\n" + teamWins[1] + "\n";
            }

            if (teamWinNum == 2)
            {
                rankingString = "Team 2 \nTeam 1\n";
                winsString = teamWins[1] + "\n" + teamWins[0] + "\n";
            }
        }

        Debug.Log("Ranking names: " + rankingString);
        Debug.Log("Ranking wins: " + winsString);

        RpcShowRanking(rankingString, winsString);
    }

    [ClientRpc]
    private void RpcShowRoundPanel()
    {
        GameModeUIController gameModeUIController = FindObjectOfType<GameModeUIController>();

        if (gameModeUIController != null)
        {
            gameModeUIController.DisplayRoundPanel();
        }
    }

    [ClientRpc]
    private void RpcStopShowRoundPanel()
    {
        GameModeUIController gameModeUIController = FindObjectOfType<GameModeUIController>();

        if (gameModeUIController != null)
        {
            gameModeUIController.StopDisplayRoundPanel();
        }
    }

    [ClientRpc]
    private void RpcShowRanking(string rankings, string wins)
    {
        GameModeUIController gameModeUIController = FindObjectOfType<GameModeUIController>();

        if (gameModeUIController != null)
        {
            gameModeUIController.DisplayRanking(rankings, wins);
        }
    }

    [ClientRpc]
    private void RpcStopShowRanking()
    {
        GameModeUIController gameModeUIController = FindObjectOfType<GameModeUIController>();

        if (gameModeUIController != null)
        {
            gameModeUIController.StopDisplayRanking();
        }
    }

    [ClientRpc]
    private void RpcDisableGameInteraction()
    {
        // Call the disable game interaction for all players
        foreach (PlayerObjectController player in Manager.GamePlayers)
        {
            player.GetComponent<PlayerController>().enabled = false;
        }
    }

    [ClientRpc]
    private void RpcResetGame()
    {
        // Call the reset function for all players
        foreach (PlayerObjectController player in Manager.GamePlayers)
        {
            player.GetComponent<PlayerController>().enabled = true;
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
