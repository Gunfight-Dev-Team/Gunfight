using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameModeUIController : MonoBehaviour
{
    public Text Winner;

    public void DisplayWinner(string newText)
    {
        Winner.enabled = true;
        Winner.text = newText;
    }

    public void StopDisplayWinner()
    {
        Winner.enabled = false;
    }
}
