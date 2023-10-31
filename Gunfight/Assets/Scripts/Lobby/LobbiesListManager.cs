using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class LobbiesListManager : MonoBehaviour
{
    public static LobbiesListManager instance;

    public GameObject lobbiesMenu;
    public GameObject lobbyDataItemPrefab;
    public GameObject lobbyListContent;
    public Text buttonText;

    public GameObject searchBar;

    public GameObject title;

    public GameObject lobbiesButton, hostButton, quickstartButton, backButton, singleplayerButton;

    public List<GameObject> listOfLobbies = new List<GameObject> ();

    private int totalLobbies;

    private bool inLobbyList = false;

    private void Awake()
    {
        if (instance == null) { instance = this; }
    }

    public void GoBack()
    {
        if (inLobbyList)
        {
            lobbiesButton.SetActive(true);
            hostButton.SetActive(true);
            quickstartButton.SetActive(true);
            title.SetActive(true);
            singleplayerButton.SetActive(true);
            lobbiesMenu.SetActive(false);
            backButton.SetActive(true);
            DestroyLobbies();
            inLobbyList = false;
            buttonText.enabled = true;
        }
        else
        {
            SceneManager.LoadScene("Start");
        }
    }

    public void toLobbyList()
    {
        inLobbyList = true;
        quickstartButton.SetActive(false);
        hostButton.SetActive(false);
        singleplayerButton.SetActive(false);
        lobbiesButton.SetActive(false);
        buttonText.enabled = false;
        title.SetActive(false);

        lobbiesMenu.SetActive(true);

        SteamLobby.Instance.GetLobbiesList();
    }

    public void DisplayLobbies(List<CSteamID> lobbyIDs, LobbyDataUpdate_t result)
    {
        for(int i = 0; i < lobbyIDs.Count; i++)
        {
            if (lobbyIDs[i].m_SteamID == result.m_ulSteamIDLobby)
            {
                GameObject createdItem = Instantiate(lobbyDataItemPrefab);

                createdItem.GetComponent<LobbyDataEntry>().lobbyID = (CSteamID)lobbyIDs[i].m_SteamID;

                createdItem.GetComponent<LobbyDataEntry>().lobbyName =
                    SteamMatchmaking.GetLobbyData((CSteamID)lobbyIDs[i].m_SteamID, "name");

                createdItem.GetComponent<LobbyDataEntry>().lobbyPlayerCount = 
                    SteamMatchmaking.GetNumLobbyMembers((CSteamID)lobbyIDs[i].m_SteamID);

                createdItem.GetComponent<LobbyDataEntry>().SetLobbyData();

                createdItem.transform.SetParent(lobbyListContent.transform);
                createdItem.transform.localScale = Vector3.one;

                listOfLobbies.Add(createdItem);
                totalLobbies = listOfLobbies.Count;
            }
        }
    }

    public void SearchLobbies()
    {
        string searchText = searchBar.GetComponent<TMP_InputField>().text;
        int searchLength = searchText.Length;

        int searchedLobbies = 0;

        foreach (GameObject lobby in listOfLobbies)
        {
            searchedLobbies++;
            if (lobby.GetComponent<LobbyDataEntry>().lobbyName.Length >= searchLength)
            {
                if (searchText.ToLower() == lobby.GetComponent<LobbyDataEntry>().lobbyName.Substring(0, searchLength).ToLower())
                {
                    lobby.SetActive(true);
                }
                else
                {
                    lobby.SetActive(false);
                }
            }
        }
    }

    public void DestroyLobbies()
    {
        foreach(GameObject l in listOfLobbies)
        {
            Destroy(l);
        }
        listOfLobbies.Clear();
    }

    public void RefreshLobbies()
    {
        DestroyLobbies();
        SteamLobby.Instance.GetLobbiesList();
    }
}
