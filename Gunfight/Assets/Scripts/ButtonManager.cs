using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonManager : MonoBehaviour
{
    public void HostLobbyButton()
    {
        SteamLobby.Instance.HostLobby();
    }

    public void QuickStartButton()
    {
        SteamLobby.Instance.QuickStart();
    }
}
