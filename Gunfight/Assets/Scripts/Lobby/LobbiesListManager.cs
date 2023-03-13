using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using UnityEngine.SceneManagement;

public class LobbiesListManager : MonoBehaviour
{
    public static LobbiesListManager instance;

    public GameObject lobbiesMenu;
    public GameObject lobbyDataItemPrefab;
    public GameObject lobbyListContent;

    public GameObject title;

    public GameObject lobbiesButton, hostButton, backButton;

    public List<GameObject> listOfLobbies = new List<GameObject> ();

    private void Awake()
    {
        if (instance == null) { instance = this; }
    }

    public void GetListOfLobbies()
    {
        lobbiesButton.SetActive (false);
        hostButton.SetActive (false);
        title.SetActive(false);
        backButton.SetActive(false);
        lobbiesMenu.SetActive (true);

        SteamLobby.Instance.GetLobbiesList();
    }

    public void GoBack()
    {
        lobbiesButton.SetActive(true);
        hostButton.SetActive(true);
        title.SetActive(true);
        backButton.SetActive(true);
        lobbiesMenu.SetActive(false);

        DestroyLobbies();
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
