using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public interface IGameMode
{
    void StartRound();
    void EndRound();
    void ToLobby();
    void PlayerDied(PlayerController player);
    void QuitGame();
    IEnumerator QuitCountdown();
    IEnumerator DelayedEndRound();
    IEnumerator PreroundCountdown();
    void CheckWinCondition(int oldAliveNum, int newAliveNum);
    void SpawnWeaponsInGame();
    void DeleteWeaponsInGame();
    bool CheckIfGameNeedsStart();
    void InitializeGameMode();
    void DecrementCurrentNumberOfEnemies();


    bool GetUseCards();
    void SetUseCards(bool usingCards);
    int GetAliveNum();
    void SetAliveNum(int num);
    void SetTotalRounds(int totalRounds);
    void SetQuitClicked(bool b);

    // temporary UI RPC methods
    [ClientRpc]
    void RpcShowRoundPanel();
    [ClientRpc]
    void RpcStopShowRoundPanel();
    [ClientRpc]
    void RpcShowWinner(string winner);
    [ClientRpc]
    void RpcStopShowWinner();
    [ClientRpc]
    void RpcResetGame();
    [ClientRpc]
    void RpcShowCount(string count);
    [ClientRpc]
    void RpcStopShowCount();
}
