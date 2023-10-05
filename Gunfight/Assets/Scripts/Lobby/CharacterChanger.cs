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
    public string[] colorNames;
    public Image currentColorImage;
    public Image displaySprite;
    private PlayerController player;
    private GameObject playerSprite;
    public Text currentColorText;

    private void Start()
    {
        currentColorIndex = PlayerPrefs.GetInt("currentColorIndex", 0); // allows persistent variables even on game restart
        currentColorImage.color = playerColors[currentColorIndex];
        currentColorText.text = colorNames[currentColorIndex];
        LobbyController.Instance.LocalPlayerController.CmdUpdatePlayerColor(currentColorIndex);
        player = GameObject.Find("LocalGamePlayer").GetComponent<PlayerController>();
        player.SwitchSkin(currentColorIndex);
        playerSprite = player.transform.Find("Sprite").gameObject;
    }

    private void Update()
    {
        displaySprite.sprite = playerSprite.GetComponent<SpriteRenderer>().sprite;
    }

    public void NextColor()
    {
        if(currentColorIndex < playerColors.Length -1)
        {
            currentColorIndex++;
            PlayerPrefs.SetInt("currentColorIndex", currentColorIndex);
            currentColorImage.color = playerColors[currentColorIndex];
            currentColorText.text = colorNames[currentColorIndex];
            LobbyController.Instance.LocalPlayerController.CmdUpdatePlayerColor(currentColorIndex);
            player.SwitchSkin(currentColorIndex);
        }
    }

    public void PrevColor()
    {
        if (currentColorIndex > 0)
        {
            currentColorIndex--;
            PlayerPrefs.SetInt("currentColorIndex", currentColorIndex);
            currentColorImage.color = playerColors[currentColorIndex];
            currentColorText.text = colorNames[currentColorIndex];
            LobbyController.Instance.LocalPlayerController.CmdUpdatePlayerColor(currentColorIndex);
            player.SwitchSkin(currentColorIndex);
        }
    }
}
