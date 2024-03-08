using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Mirror;

public class SettingsManager : NetworkBehaviour
{
    public GameObject QuitButton;

    public GameObject SettingPanel;

    public PlayerObjectController LocalPlayerController;

    private bool inSettings = false;

    // Start is called before the first frame update
    void Start()
    {
        if (LocalPlayerController == null)
        {
            LocalPlayerController = GameObject.Find("LocalGamePlayer").GetComponent<PlayerObjectController>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!inSettings)
            {
                // display the settings panel
                ShowSettingsPanel(true);
            }
            else
            {
                // stop displaying settings panel
                ShowSettingsPanel(false);
            }
        }
    }

    public void ShowSettingsPanel(bool tOrF)
    {
        SettingPanel.SetActive(tOrF);
        QuitButton.SetActive(tOrF);
        inSettings = tOrF;
        LocalPlayerController.GetComponent<PlayerController>().enabled = !tOrF;
    }

    public void QuitGameToMatchmaking()
    {
        Invoke("loadQuitGameToMatchmaking", 0.75f);
    }

    private void loadQuitGameToMatchmaking()
    {
        if (!isClient) return;
        CmdPlayerQuit();
        Invoke("PlayerQuitGame", 0.1f);
    }

    private void PlayerQuitGame()
    {
        LocalPlayerController.Quit();
    }

    [Command(requiresAuthority = false)]
    public void CmdPlayerQuit()
    {
        Debug.Log("sending player left the game to server");
        GameModeManager.Instance.PlayerQuit(); 
    }
}