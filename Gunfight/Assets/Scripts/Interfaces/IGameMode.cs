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
    IEnumerator QuitCountdown();
    IEnumerator DelayedEndRound();
    IEnumerator PreroundCountdown();
    void CheckWinCondition(int oldAliveNum, int newAliveNum);
    void SpawnWeaponsInGame();
    void DeleteWeaponsInGame();
    bool CheckIfGameNeedsStart();
    void InitializeGameMode();

    //bool getHasGameStarted();
    //void setHasGameStarted(bool hasGameStarted);
    //int getAliveNum();
    //void setTotalRounds(int rounds);
    //void setPlayerCount(int numPlayers);

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
