using Mirror;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class SurvivalMode : NetworkBehaviour, IGameMode
{
    public static GameModeManager Instance;
    public MapManager mapManager;
    public CardManager cardManager;

    private CustomNetworkManager manager;

    public GameObject enemyPrefab;
    public int startingNumberOfEnemies = 4;
    public float enemyMultiplier = 1.15f;
    public int currentRoundNumberOfEnemies;

    public int playerCount;
    public bool hasGameStarted = false;
    public bool useCards = false;

    [SyncVar(hook = nameof(CheckWinCondition))]
    public int currentNumberOfEnemies;
    [SyncVar(hook = nameof(CheckWinCondition))]
    public int aliveNum; // get this from lobby

    [SyncVar]
    public int currentRound = 0; // keeps track of the current round
    public int totalRounds = 3; // keeps track of total amount of rounds

    public bool quitClicked = false;

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

    //-------------Wave Mode-exclusive Methods--------------

    private void initEnemy()
    {
        if (!isServer)
        {
            return;
        }
        currentNumberOfEnemies = startingNumberOfEnemies;
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

    //------------------Game Mode Interface Methods------------------------------

    public bool CheckIfGameNeedsStart()
    {
        return !hasGameStarted && (SceneManager.GetActiveScene().name != "Lobby") && aliveNum != 0;
    }

    public void InitializeGameMode()
    {
        mapManager = GameObject.Find("MapManager").GetComponent<MapManager>();

        // for wave mode only
        totalRounds = 9999;
        initEnemy();

        //for all game modes (checked isServer here, removed b/c redundant, if bugged check here)
        playerCount = aliveNum;
        hasGameStarted = true;
        StartRound();
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
        if (!isServer) { return; }
        DeleteWeaponsInGame();
        //checked if server here? doesn't seem needed but check here if bugged
        RpcResetGame();
        SpawnWeaponsInGame();
        currentRoundNumberOfEnemies = Mathf.RoundToInt(currentRoundNumberOfEnemies * enemyMultiplier);
        currentNumberOfEnemies = currentRoundNumberOfEnemies;
        StartRound();
        spawnEnemies();
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

    public void QuitGame()
    {
        // quits back to the lobby
        GameModeUIController gameModeUIController = FindObjectOfType<GameModeUIController>();
        gameModeUIController.StopDisplayQuitButton();
        RpcStopShowWinner();
        RpcStopShowRoundPanel();
        quitClicked = false;
        ToLobby();
    }

    public IEnumerator DelayedEndRound()
    {
        if (isServer && SceneManager.GetActiveScene().name != "Lobby" &&
            currentNumberOfEnemies <= 0) // changed from curNumNME != startingNum 
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

            // If no enemy, end round (bug source??)
            if (currentNumberOfEnemies <= 0)
            {
                cardManager.RpcShowCardPanel();
                RpcShowWinner("Round: " + currentRound);
                yield return new WaitForSeconds(10.0f);
                RpcStopShowWinner();
                cardManager.RpcStopCardPanel();

                StartCoroutine(PreroundCountdown());
                yield return new WaitForSeconds(5f);
            }
            EndRound();
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

            // Reduce the countdown time
            countdownTime -= Time.deltaTime;
        }

        // Clear the countdown text when the countdown is complete
        RpcStopShowCount();
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

    public int GetAliveNum()
    {
        return this.aliveNum;
    }

    public void SetAliveNum(int num)
    {
        this.aliveNum = num;
    }

    public void SetUseCards(bool usingCards)
    {
        this.useCards = usingCards;
    }

    public bool GetUseCards()
    {
        return this.useCards;
    }

    public void SetTotalRounds(int rounds)
    {
        this.totalRounds = rounds;
    }

    public void DecrementCurrentNumberOfEnemies()
    {
        this.currentNumberOfEnemies--;
    }

    public void SetQuitClicked(bool b)
    {
        this.quitClicked = b;
    }

    //------------------------------------------------------------------
    //--------------------temporary UI RPC Methods----------------------
    //------------------------------------------------------------------

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
