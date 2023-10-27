using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class MapController : NetworkBehaviour
{
    public GameObject LocalPlayerObject;
    public string[] mapNames;
    public int currentMapIndex;
    public Text currentMapText;
    [SyncVar(hook = nameof(UpdateMapName))] public string mapNameSynced = "Game";

    private Button prevMap;
    private Button nextMap;
    private PlayerObjectController LocalPlayerController;

    private void Start()
    {
        currentMapIndex = 0;
        UpdateMapVariables();

        LocalPlayerObject = GameObject.Find("LocalGamePlayer");
        LocalPlayerController = LocalPlayerObject.GetComponent<PlayerObjectController>();
        prevMap = GameObject.Find("PrevMapButton")?.GetComponent<Button>();
        nextMap = GameObject.Find("NextMapButton")?.GetComponent<Button>();

        if (LocalPlayerController.PlayerIdNumber == 1)
        {
            prevMap.interactable = true;
            nextMap.interactable = true;
        }
        else
        {
            prevMap.interactable = false;
            nextMap.interactable = false;
        }
    }

    public void NextMap()
    {
        if(currentMapIndex < mapNames.Length-1)
        {
            currentMapIndex++;
            UpdateMapVariables();
            CmdSendMessageToPlayers(mapNames[currentMapIndex]);
        }
    }

    public void PrevMap()
    {
        if (currentMapIndex > 0)
        {
            currentMapIndex--;
            UpdateMapVariables();
            CmdSendMessageToPlayers(mapNames[currentMapIndex]);
        }
    }

    public void UpdateMapVariables()
    {
        currentMapText.text = mapNames[currentMapIndex];
        LobbyController.Instance.MapName = mapNames[currentMapIndex];
    }

    public void UpdateMapName(string oldValue, string newValue)
    {
        if(isServer)
        {
            mapNameSynced = newValue;
        }
        if(isClient && (oldValue != newValue))
        {
            currentMapText.text = newValue;
        }
    }

    [Command(requiresAuthority = false)]
    void CmdSendMessageToPlayers(string newMessage)
    {
        UpdateMapName(mapNameSynced, newMessage);
    }
}
