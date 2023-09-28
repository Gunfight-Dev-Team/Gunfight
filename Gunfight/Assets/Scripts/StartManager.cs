using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartManager : MonoBehaviour
{
    public GameObject

            StartButton,
            ControlsButton,
            SettingsButton,
            GraphicButton,
            SoundButton,
            QuitButton,
            BackButton,
            Title;

    public GameObject ControlsImage;

    public GameObject SettingPage;

    public GameObject SoundPage;

    public GameObject GraphicPage;

    private bool inControl;

    // Start is called before the first frame update
    void Start()
    {
        init();
    }

    // Update is called once per frame
    void Update()
    {
        if (inControl)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                init();
            }
        }
    }

    public void toControl()
    {
        Title.SetActive(false);
        StartButton.SetActive(false);
        ControlsButton.SetActive(false);
        SettingsButton.SetActive(false);
        QuitButton.SetActive(false);
        ControlsImage.SetActive(true);
        BackButton.SetActive(true);
        inControl = true;
    }

    public void toSetting()
    {
        Title.SetActive(false);
        StartButton.SetActive(false);
        ControlsButton.SetActive(false);
        SettingsButton.SetActive(false);
        QuitButton.SetActive(false);
        SettingPage.SetActive(true);
        BackButton.SetActive(true);
    }

    public void toSound()
    {
        SettingPage.SetActive(false);
        SoundPage.SetActive(true);
    }

    public void toGraphic()
    {
        SettingPage.SetActive(false);
        GraphicPage.SetActive(true);
    }

    public void Back()
    {
        if (SoundPage.activeSelf || GraphicPage.activeSelf)
        {
            toSetting();
            SoundPage.SetActive(false);
            GraphicPage.SetActive(false);
        }
        else if (SettingPage.activeSelf || ControlsImage.activeSelf)
        {
            init();
            SettingPage.SetActive(false);
        }
    }



    public void init()
    {
        Title.SetActive(true);
        StartButton.SetActive(true);
        ControlsButton.SetActive(true);
        SettingsButton.SetActive(true);
        QuitButton.SetActive(true);
        SettingPage.SetActive(false);
        ControlsImage.SetActive(false);
        BackButton.SetActive(false);
        inControl = false;
    }

    public void toStart()
    {
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
