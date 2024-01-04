using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class CardManager : NetworkBehaviour
{
    [SyncVar]
    public int card1Vote = 0;
    public int card2Vote = 0;
    public int card3Vote = 0;
    public int totalVote = 0;

    public Button card1;
    public Button card2;
    public Button card3;

    private CardUIController cardUIController;

    void Start()
    {
        cardUIController = FindObjectOfType<CardUIController>();
        Button btn1 = card1.GetComponent<Button>();
        Button btn2 = card2.GetComponent<Button>();
        Button btn3 = card3.GetComponent<Button>();
        btn1.onClick.AddListener(TaskOnClickBtn1);
        btn2.onClick.AddListener(TaskOnClickBtn2);
        btn3.onClick.AddListener(TaskOnClickBtn3);
    }

    void TaskOnClickBtn1()
    {
        // when first card is clicked
        Debug.Log("Card 1 pressed");
        if (cardUIController != null)
        {
            cardUIController.DisableCards2and3();
        }

        // increase votes
        card1Vote++;
        totalVote++;
    }

    void TaskOnClickBtn2()
    {
        // when second card is clicked
        Debug.Log("Card 2 pressed");
        if (cardUIController != null)
        {
            cardUIController.DisableCards1and3();
        }

        // increase votes
        card2Vote++;
        totalVote++;
    }

    void TaskOnClickBtn3()
    {
        // when third card is clicked
        Debug.Log("Card 3 pressed");
        if (cardUIController != null)
        {
            cardUIController.DisableCards1and2();
        }

        // increase votes
        card3Vote++;
        totalVote++;
    }
}
