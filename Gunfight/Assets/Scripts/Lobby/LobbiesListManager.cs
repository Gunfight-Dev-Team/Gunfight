using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using UnityEngine.SceneManagement;
using TMPro;

public class LobbiesListManager : MonoBehaviour
{
    public static LobbiesListManager instance;

    public GameObject lobbiesMenu;
    public GameObject lobbyDataItemPrefab;
    public GameObject lobbyListContent;

    public GameObject searchBar;

    public GameObject title;

    public GameObject lobbiesButton, hostButton, quickstartButton, backButton;

    public List<GameObject> listOfLobbies = new List<GameObject> ();

    private int totalLobbies;

    private void Awake()
    {
        if (instance == null) { instance = this; }
    }

    public void GetListOfLobbies()
    {
        lobbiesButton.SetActive (false);
        hostButton.SetActive (false);
        title.SetActive(false);
        lobbiesMenu.SetActive (true);
        backButton.SetActive(false);

        SteamLobby.Instance.GetLobbiesList();
    }

    public void GoBack()
    {
        lobbiesButton.SetActive(true);
        hostButton.SetActive(true);
        title.SetActive(true);
        lobbiesMenu.SetActive(false);
        backButton.SetActive(true);

        DestroyLobbies();
    }

    public void BackPage()
    {
        SceneManager.LoadScene("Start");
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
}
