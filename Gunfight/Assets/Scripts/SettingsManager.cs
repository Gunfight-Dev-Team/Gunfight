using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
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
        LocalPlayerController.Quit();
    }
}
