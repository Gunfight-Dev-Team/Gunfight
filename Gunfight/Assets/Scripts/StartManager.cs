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

    public GameObject ControlsPage;

    public GameObject SettingPage;

    public GameObject SoundPage;

    public GameObject GraphicPage;

    private bool inControl;

    private Dictionary<Transform, TransformData> initialTransforms = new Dictionary<Transform, TransformData>();

    // Helper class to store RectTransform data
    public class TransformData
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;

        public TransformData(Transform transform)
        {
            position = transform.GetComponent<RectTransform>().localPosition;
            rotation = transform.GetComponent<RectTransform>().localRotation;
            scale = transform.localScale;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        init();
        StoreInitialPositions();
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

    void StoreInitialPositions()
    {
        initialTransforms.Add(StartButton.transform, new TransformData(StartButton.transform));
        initialTransforms.Add(ControlsButton.transform, new TransformData(ControlsButton.transform));
        initialTransforms.Add(SettingsButton.transform, new TransformData(SettingsButton.transform));
        initialTransforms.Add(GraphicButton.transform, new TransformData(GraphicButton.transform));
        initialTransforms.Add(SoundButton.transform, new TransformData(SoundButton.transform));
        initialTransforms.Add(BackButton.transform, new TransformData(BackButton.transform));
    }

    public void toControl()
    {
        SettingPage.SetActive(false);
        ControlsPage.SetActive(true);
        BackButton.SetActive(true);
        inControl = true;
    }

    public void toSetting()
    {
        Invoke("loadSetting", 0.75f);
    }

    public void loadSetting()
    {
        Title.SetActive(false);
        StartButton.SetActive(false);
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

    public void toDiscord()
    {
        Application.OpenURL("https://discord.gg/pweP89xGts");
    }

    public void Back()
    {
        if (SoundPage.activeSelf || GraphicPage.activeSelf || ControlsPage.activeSelf)
        {
            toSetting();
            SoundPage.SetActive(false);
            GraphicPage.SetActive(false);
            ControlsPage.SetActive(false);
        }
        else if (SettingPage.activeSelf)
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
        ControlsPage.SetActive(false);
        BackButton.SetActive(false);
        inControl = false;

        // Reset button positions
        foreach (var kvp in initialTransforms)
        {
            kvp.Key.GetComponent<RectTransform>().localPosition = kvp.Value.position;
            kvp.Key.GetComponent<RectTransform>().localRotation = kvp.Value.rotation;
            kvp.Key.localScale = kvp.Value.scale;

            Rigidbody rb = kvp.Key.gameObject.GetComponent<Rigidbody>();
            Transform shot = kvp.Key.gameObject.transform.Find("UIshot(Clone)");
            if (rb != null)
            {
                Destroy(rb);
            }
            if (shot != null)
            {
                Destroy(shot.gameObject);
            }
        }
    }

    public void toStart()
    {
        Invoke("LoadMainMenu", 0.75f);
    }

    private void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }
    public void Quit()
    {
        Application.Quit();
    }
}
