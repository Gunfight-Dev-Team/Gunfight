using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGameMode
{
    void StartRound();
    void EndRound();
    void ToLobby();
    void PlayerDied(PlayerController player);
    IEnumerator QuitCountdown();
    IEnumerator DelayedEndRound();
    IEnumerator PreroundCountdown();
    void CheckWinCondition(int oldAliveNum, int newAliveNum)
    {
        StartCoroutine(DelayedEndRoundSingle());
    }
}
