using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    public GameObject LocalPlayerObject;
    public PlayerObjectController LocalPlayerController;

    // Start is called before the first frame update
    void Start()
    {
        LocalPlayerObject = GameObject.Find("LocalGamePlayer");
        LocalPlayerController =
            LocalPlayerObject.GetComponent<PlayerObjectController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            LocalPlayerController.CanStartGame("Game");
        }
    }
}
