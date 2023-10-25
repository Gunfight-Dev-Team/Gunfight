using Mirror;
using System;
using UnityEngine;
using UnityEngine.UI;
using Steamworks;

public class ChatController : NetworkBehaviour
{
    [SerializeField] private Text chatText = null;
    [SerializeField] private InputField inputField = null;

    // When a new message is added, update the Scroll View's Text to include the new message
    private void HandleNewMessage(string message, bool alignedLeft)
    {
        if (alignedLeft)
            chatText.alignment = TextAnchor.MiddleLeft;
        else
            chatText.alignment = TextAnchor.MiddleRight;
        chatText.text += message + '\n';
    }

    // When a client hits the enter button, send the message in the InputField
    [Client]
    public void Send()
    {
        if (!Input.GetKeyDown(KeyCode.Return)) { return; }
        if (string.IsNullOrWhiteSpace(inputField.text)) { return; }
        CmdSendMessage(inputField.text);
        inputField.text = string.Empty;
    }

    [Command(requiresAuthority = false)]
    private void CmdSendMessage(string message)
    {
        // Validate message
        Debug.Log("Command" + message);
        RpcHandleMessage("<b>" + SteamFriends.GetPersonaName().ToString() + "</b>: " + message);
    }

    [ClientRpc]
    private void RpcHandleMessage(string message)
    {
        if (isServer)
        {
            HandleNewMessage(message, false);
        }
        else
        {
            HandleNewMessage(message, true);
        }
    }

}