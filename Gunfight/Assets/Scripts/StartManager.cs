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
            QuitButton,
            BackButton,
            Title;

    public GameObject ControlsImage;

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
        QuitButton.SetActive(false);
        ControlsImage.SetActive(true);
        BackButton.SetActive(true);
        inControl = true;
    }

    public void init()
    {
        Title.SetActive(true);
        StartButton.SetActive(true);
        ControlsButton.SetActive(true);
        QuitButton.SetActive(true);
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
