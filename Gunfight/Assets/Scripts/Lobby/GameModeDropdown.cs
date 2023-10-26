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
        Debug.Log("1");
        // Call a command to change the Dropdown value on the server
        CmdChangeDropdownValue(dropdown.value);
    }

    [Command(requiresAuthority = false)]
    void CmdChangeDropdownValue(int value)
    {
        Debug.Log("2");
        // Change the Dropdown value on the server
        RpcSyncDropdownValue(value);
    }

    [ClientRpc]
    void RpcSyncDropdownValue(int value)
    {
        Debug.Log("3");
        // Change the Dropdown value on all clients
        dropdown.value = value;
    }
}
