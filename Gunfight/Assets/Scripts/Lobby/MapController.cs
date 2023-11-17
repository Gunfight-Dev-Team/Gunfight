using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class MapController : NetworkBehaviour
{
    public GameObject LocalPlayerObject;
    public string[] mapNames;
    [SyncVar]
    public int currentMapIndex;
    public Text currentMapText;

    private Button prevMap;
    private Button nextMap;
    private PlayerObjectController LocalPlayerController;

    private void Start()
    {
        if (isServer)
        {
            currentMapIndex = 0;
            RpcUpdateMapVariables(mapNames[currentMapIndex]);
        }
        else
        {
            currentMapText.text = mapNames[currentMapIndex];
            LobbyController.Instance.MapName = mapNames[currentMapIndex];
        }

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
            RpcUpdateMapVariables(mapNames[currentMapIndex]);
        }
        else
        {
            currentMapIndex = 0;
            RpcUpdateMapVariables(mapNames[currentMapIndex]);
        }
    }

    public void PrevMap()
    {
        if (currentMapIndex > 0)
        {
            currentMapIndex--;
            RpcUpdateMapVariables(mapNames[currentMapIndex]);
        }
        else
        {
            currentMapIndex = mapNames.Length-1;
            RpcUpdateMapVariables(mapNames[currentMapIndex]);
        }
    }

    [ClientRpc]
    public void RpcUpdateMapVariables(string newMapName)
    {
        currentMapText.text = newMapName;
        LobbyController.Instance.MapName = newMapName;
    }
}
