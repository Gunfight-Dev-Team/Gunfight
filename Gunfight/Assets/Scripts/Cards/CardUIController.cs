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
        card1.gameObject.SetActive(true);
        card2.gameObject.SetActive(true);
        card3.gameObject.SetActive(true);
        card1.interactable = true;
        card2.interactable = true;
        card3.interactable = true;
    }

    public void StopDisplayCardPanel()
    {
        CardPanel.SetActive(false);
        card1.gameObject.SetActive(true);
        card2.gameObject.SetActive(true);
        card3.gameObject.SetActive(true);
        card1.interactable = false;
        card2.interactable = false;
        card3.interactable = false;
    }

    public void DisableCards()
    {
        card1.interactable = false;
        card2.interactable = false;
        card3.interactable = false;
    }

    public void DisplayCard1()
    {
        card2.gameObject.SetActive(false);
        card3.gameObject.SetActive(false);
    }

    public void DisplayCard2()
    {
        card1.gameObject.SetActive(false);
        card3.gameObject.SetActive(false);
    }

    public void DisplayCard3()
    {
        card1.gameObject.SetActive(false);
        card2.gameObject.SetActive(false);
    }
}
