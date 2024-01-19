using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameModeUIController : MonoBehaviour
{
    public Text Winner;
    public Text Countdown;
    
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
}
