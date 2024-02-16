using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameModeUIController : MonoBehaviour
{
    public Text Winner;
    public Text Countdown;
    public Text Timer;
    public Text RoundNumber;
    public Text Ranking;
    public Text Wins;
    public GameObject RoundStats;
    public Button quitButton;
    public GameObject quit;

    public void DisplayRoundPanel()
    {
        RoundStats.SetActive(true);
    }

    public void StopDisplayRoundPanel()
    {
        RoundStats.SetActive(false);
    }
    
    public void DisplayWinner(string newText)
    {
        Winner.enabled = true;
        Winner.text = newText;
    }

    public void StopDisplayWinner()
    {
        Winner.enabled = false;
    }

    public void DisplayCount(string newText)
    {
        Countdown.enabled = true;
        Countdown.text = newText;
    }

    public void StopDisplayCount()
    {
        Countdown.enabled = false;
    }

    public void DisplayTimer(string newText)
    {
        Timer.enabled = true;
        Timer.text = newText;
    }

    public void StopDisplayTimer()
    {
        Timer.enabled = false;
    }

    public void DisplayRoundNumber(string newText)
    {
        RoundNumber.enabled = true;
        RoundNumber.text = newText;
    }

    public void StopDisplayRoundNumber()
    {
        RoundNumber.enabled = false;
    }

    public void DisplayRanking(string newRanking, string newWins)
    {
        Ranking.enabled = true;
        Wins.enabled = true;
        Ranking.text = newRanking;
        Wins.text = newWins;
    }

    public void StopDisplayRanking()
    {
        Ranking.enabled = false;
        Wins.enabled = false;
    }

    public void DisplayQuitButton()
    {
        quit.SetActive(true);
        quitButton.enabled = true;
        quitButton.interactable = true;
    }

    public void StopDisplayQuitButton()
    {
        quit.SetActive(false);
        quitButton.enabled = false;
        quitButton.interactable = false;
    }
}
