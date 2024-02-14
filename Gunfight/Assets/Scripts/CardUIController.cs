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
        InteractableCards(true);
    }

    public void StopDisplayCardPanel()
    {
        CardPanel.SetActive(false);
        card1.gameObject.SetActive(true);
        card2.gameObject.SetActive(true);
        card3.gameObject.SetActive(true);
        InteractableCards(false);
    }

    public void InteractableCards(bool tOrF)
    {
        card1.interactable = tOrF;
        card2.interactable = tOrF;
        card3.interactable = tOrF;
    }

    public void StopDisplayCards(Button firstCard, Button secondCard)
    {
        firstCard.gameObject.SetActive(false);
        secondCard.gameObject.SetActive(false);
    }
}
