using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class CardManager : NetworkBehaviour
{
    public int card1Vote = 0;
    public int card2Vote = 0;
    public int card3Vote = 0;

    

    // [ClientRpc]
    // public void RPCShowCardPanel()
    // {
    //     CardUIController cardUIController = FindObjectOfType<CardUIController>();

    //     if (cardUIController != null)
    //     {
    //         cardUIController.DisplayCardPanel();
    //     }
    // }

    // [ClientRpc]
    // public void RPCStopCardPanel()
    // {
    //     CardUIController cardUIController = FindObjectOfType<CardUIController>();

    //     if (cardUIController != null)
    //     {
    //         cardUIController.StopDisplayCardPanel();
    //     }
    // }
}
