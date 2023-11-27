using System.Collections;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

public class GameModeManager : NetworkBehaviour
{
    public static GameModeManager Instance;
    public MapManager mapManager;

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
                SceneManager.LoadScene("Lobby");
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
        // yield return StartCoroutine(CardCountdown());

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

    private IEnumerator DelayedEndRoundSingle()
    {
        if (isServer && SceneManager.GetActiveScene().name != "Lobby" && 
            currentNumberOfEnemies != startingNumberOfEnemies)
        {
            // If no enemy, end round 
            if (currentNumberOfEnemies <= 0)
            {
                StartCoroutine(Countdown());
                yield return new WaitForSeconds(5f);
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
        if (gameMode != GameMode.SinglePlayer)
        {
            StartCoroutine(CardCountdown()); // card mechanic
            // StartCoroutine(DelayedEndRound());
        }
    }

    void CheckWinConditionSingle(int oldAliveNum, int newAliveNum)
    {
        StartCoroutine(DelayedEndRoundSingle());
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

    // public void StartCards()
    // {
    //     RpcShowCardPanel();
    //     StartCoroutine(CardCountdown());
    //     RpcStopCardPanel();
    // }

    private IEnumerator CardCountdown()
    {
        RpcShowCardPanel();
        float countdownTime = 10f;

        while (countdownTime > 0)
        {
            // Wait for the next frame
            yield return null;

            // Reduce the countdown time
            countdownTime -= Time.deltaTime;
        }
        RpcStopCardPanel();
        StartCoroutine(DelayedEndRound());
    }

    [ClientRpc]
    public void RpcShowCardPanel()
    {
        CardUIController cardUIController = FindObjectOfType<CardUIController>();

        if (cardUIController != null)
        {
            cardUIController.DisplayCardPanel();
        }
    }

    [ClientRpc]
    public void RpcStopCardPanel()
    {
        CardUIController cardUIController = FindObjectOfType<CardUIController>();

        if (cardUIController != null)
        {
            cardUIController.StopDisplayCardPanel();
        }
    }
}
