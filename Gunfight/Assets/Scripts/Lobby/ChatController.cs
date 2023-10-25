using Mirror;
using System;
using UnityEngine;
using UnityEngine.UI;
using Steamworks;

public class ChatController : NetworkBehaviour
{
    [SerializeField] private Text chatTextLeft = null;
    [SerializeField] private Text chatTextRight = null;
    [SerializeField] private InputField inputField = null;

    // When a new message is added, update the Scroll View's Text to include the new message
    private void HandleNewMessage(string message, bool isLeftAligned)
    {
        if(isLeftAligned)
        {
            chatTextLeft.text += '\n' + message;
        }
        else
        {
            chatTextRight.text += '\n' + message;
        }
        
    }

    // When a client hits the enter button, send the message in the InputField
    [Client]
    public void Send()
    {
        if (!Input.GetKeyDown(KeyCode.Return)) { return; }
        if (string.IsNullOrWhiteSpace(inputField.text)) { return; }
        CmdSendMessage(inputField.text, SteamFriends.GetPersonaName().ToString());
        inputField.text = string.Empty;
    }

    [Command(requiresAuthority = false)]
    private void CmdSendMessage(string message, string name)
    {
        if(isServer)
        {
            RpcHandleMessage("<b>" + name + "</b>: " + message, false);
        }
        else
        {
            RpcHandleMessage("<b>" + name + "</b>: " + message, true);
        }
        
    }

    [ClientRpc]
    private void RpcHandleMessage(string message, bool isLeftAligned)
    {
        HandleNewMessage(message, isLeftAligned);
    }

}