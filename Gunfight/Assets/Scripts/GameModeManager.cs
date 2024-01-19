using System.Collections;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

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
                controller.updateSpeed(currentRound);
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
                controller.updateDamage(currentRound);
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
        currentRound++; // increase round count
        Debug.Log("Round started: " + currentRound);
    }

    public void EndRound()
    {
        if (!isServer)
        {
            return;
        }
        if (gameMode != GameMode.SinglePlayer)
        {
            //if (currentRound < totalRounds) // if current round is less than total rounds
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
                RpcShowWinner("Overall Winner: " + FindOverallWinner());
                //GoToLobby();
                // SceneManager.LoadScene("Lobby");
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
            
            // If only one player is alive, end round 
            if (aliveNum <= 1)
            {
                string winner = FindWinner();
                if (!CheckOverallWin())
                {
                    RpcDisableGameInteraction();
                    cardManager.RpcShowCardPanel();
                    RpcShowWinner("Winner: " + winner);

                    // start 10s timer 
                    int count = 0;

                    while (count < 10)
                    {
                        // if everyone voted stop countdown
                        if (cardManager.CheckIfEveryoneVoted(playerCount))
                        {
                            Debug.Log("Break countdown");
                            break;
                        }
                        yield return new WaitForSeconds(1f);
                        count++;
                        Debug.Log("Card countdown: " + count);
                    }
                    
                    //if before 10s everyone voted (only time need to check if everyone voted), check for most voted card, show winning card, start game
                    // use Max functions, this will resolve ties
                    //end of 10s, check for most voted card, show winning card, begin game (hard deadline)
                    
                    // find the card voted the most
                    winningCard = cardManager.FindMaxVote();
                    Debug.Log("Winning card: " + winningCard);

                    cardManager.RpcShowWinningCard(winningCard); // only displaying the winning card
                    yield return new WaitForSeconds(5f); // pause to show winning card
                    RpcStopShowWinner();
                    cardManager.RpcStopCardPanel();
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
        foreach (PlayerObjectController player in Manager.GamePlayers)
        {
            if(player.isAlive)
            {
                player.wins++;
                return player.PlayerName;
            }
        }
        return "No one";
    }

    private string FindOverallWinner()
    {
        foreach (PlayerObjectController player in Manager.GamePlayers)
        {
            if(player.wins == totalRounds)
            {
                return player.PlayerName;
            }
        }
        return "No one";
    }

    private bool CheckOverallWin()
    {
        foreach (PlayerObjectController player in Manager.GamePlayers)
        {
            if(gameMode == GameMode.FreeForAll)
            {
                // checks if a player has the required amount of wins
                if(player.wins == totalRounds)
                {
                    return true;
                }
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

    
}
