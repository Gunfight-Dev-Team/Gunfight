using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Steamworks;
using Mirror;

public class CharacterChanger : MonoBehaviour
{
    public int currentColorIndex = 0;
    public Color[] playerColors;
    public Image currentColorImage;
    public Image displaySprite;
    private GameObject player;
    public Text currentColorText;

    private void Start()
    {
        currentColorIndex = PlayerPrefs.GetInt("currentColorIndex", 0); // allows persistent variables even on game restart
        currentColorImage.color = playerColors[currentColorIndex];
        currentColorText.text = playerColors[currentColorIndex].ToString();
        LobbyController.Instance.LocalPlayerController.CmdUpdatePlayerColor(currentColorIndex);
        GameObject player = GameObject.Find("LocalGamePlayer");
    }

    private void Update()
    {
        //displaySprite.sprite = player.transform.Find("Sprite").GetComponent<SpriteRenderer>().sprite;
    }

    public void NextColor()
    {
        if(currentColorIndex < playerColors.Length -1)
        {
            currentColorIndex++;
            PlayerPrefs.SetInt("currentColorIndex", currentColorIndex);
            currentColorImage.color = playerColors[currentColorIndex];
            currentColorText.text = playerColors[currentColorIndex].ToString();
            LobbyController.Instance.LocalPlayerController.CmdUpdatePlayerColor(currentColorIndex);
        }
    }

    public void PrevColor()
    {
        if (currentColorIndex > 0)
        {
            currentColorIndex--;
            PlayerPrefs.SetInt("currentColorIndex", currentColorIndex);
            currentColorImage.color = playerColors[currentColorIndex];
            currentColorText.text = playerColors[currentColorIndex].ToString();
            LobbyController.Instance.LocalPlayerController.CmdUpdatePlayerColor(currentColorIndex);
        }
    }
}
