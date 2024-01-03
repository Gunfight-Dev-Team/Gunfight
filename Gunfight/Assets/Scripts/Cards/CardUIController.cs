using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardUIController : MonoBehaviour
{
    public GameObject CardPanel;
    public Button card1;
    public Button card2;
    public Button card3;

    public void DisplayCardPanel()
    {
        CardPanel.SetActive(true);
        card1.interactable = true;
        card2.interactable = true;
        card3.interactable = true;
    }

    public void StopDisplayCardPanel()
    {
        CardPanel.SetActive(false);
        card1.interactable = false;
        card2.interactable = false;
        card3.interactable = false;
    }

    public void DisableCards2and3()
    {
        card2.interactable = false;
        card3.interactable = false;
    }

    public void DisableCards1and2()
    {
        card1.interactable = false;
        card2.interactable = false;
    }

    public void DisableCards1and3()
    {
        card1.interactable = false;
        card3.interactable = false;
    }
}
