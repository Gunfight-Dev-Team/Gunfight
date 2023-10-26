using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameModeDropdown : NetworkBehaviour
{
    public Dropdown dropdown;

    public void OnDropdownValueChanged()
    {
        // Call a command to change the Dropdown value on the server
        if(isServer)
            RpcSyncDropdownValue(dropdown.value);
    }

    [ClientRpc]
    void RpcSyncDropdownValue(int value)
    {
        Debug.Log("2");
        // Change the Dropdown value on all clients
        dropdown.value = value;
    }
}
