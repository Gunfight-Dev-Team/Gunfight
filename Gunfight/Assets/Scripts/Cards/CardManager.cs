using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class CardManager : NetworkBehaviour
{
    [SyncVar] public int card1Vote = 0;
    [SyncVar] public int card2Vote = 0;
    [SyncVar] public int card3Vote = 0;
    [SyncVar] public int totalVote = 0;

    public Button card1;
    public Button card2;
    public Button card3;

    private CardUIController cardUIController;

    private CustomNetworkManager manager;

    private CustomNetworkManager Manager
    {
        get
        {
            if (manager != null)
            {
                return manager;
            }
            return manager = CustomNetworkManager.singleton as CustomNetworkManager;
        }
    }

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
        // card1Vote++;
        // totalVote++;
        CMDCard1Vote();
        CMDTotalVote();
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
        // card2Vote++;
        // totalVote++;
        CMDCard2Vote();
        CMDTotalVote();
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
        // card3Vote++;
        // totalVote++;
        CMDCard3Vote();
        CMDTotalVote();
    }

    [Command]
    private void CMDCard1Vote()
    {
        card1Vote++;
        Debug.Log("Card 1 votes: " + card1Vote);
    }

    [Command]
    private void CMDCard2Vote()
    {

        card2Vote++;
        Debug.Log("Card 2 votes: " + card2Vote);
    }

    [Command]
    private void CMDCard3Vote()
    {
        card3Vote++;
        Debug.Log("Card 1 votes: " + card3Vote);
    }

    [Command]
    private void CMDTotalVote()
    {
        totalVote++;
        Debug.Log("Total votes: " + totalVote);
    }

    public int FindWinningCard()
    {
        if (isServer)
        {
            // check which card has the most votes
            if (card1Vote > card2Vote && card1Vote > card3Vote)
            {
                // if card 1 has the most votes
                return 1;
            }
            else if (card2Vote > card1Vote && card2Vote > card3Vote)
            {
                // if card 2 has the most votes
                return 2;
            }
            else if (card3Vote > card1Vote && card3Vote > card2Vote)
            {
                // if card 3 has the most votes
                return 3;
            }
            else
            {   
                // check if there is a tie
                int tie = Random.Range(0,1);
                if ((card1Vote == card2Vote) && (card1Vote != card3Vote))
                {
                    // if card 1 and 2 are equal but not 3
                    if (tie < 0.5)
                    {
                        return 1;
                    }
                    else
                    {
                        return 2;
                    }
                }
                else if ((card1Vote == card3Vote) && (card1Vote != card2Vote))
                {
                    // if card 1 and 3 are equal but not 2
                    if (tie < 0.5)
                    {
                        return 1;
                    }
                    else
                    {
                        return 3;
                    }
                }
                else if ((card2Vote == card3Vote) && (card2Vote != card1Vote))
                {
                    // if card 2 and 3 are equal but not 1
                    if (tie < 0.5)
                    {
                        return 2;
                    }
                    else
                    {
                        return 3;
                    }
                }
                else
                {
                    // if all cards are equal
                    if (tie < 0.25)
                    {
                        return 1;
                    }
                    else if (tie <= 0.5 && tie >= 0.25)
                    {
                        return 2;
                    }
                    else
                    {
                        return 3;
                    }
                }
            }
        }
        else
        {
            return 0;
        }
    }
}
