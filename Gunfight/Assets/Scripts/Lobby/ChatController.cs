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
    private string myName = "";

    private void Start()
    {
        myName = SteamFriends.GetPersonaName().ToString();
    }

    // When a new message is added, update the Scroll View's Text to include the new message
    private void HandleNewMessage(string message, string name, bool isLeftAligned)
    {
        if(isLeftAligned)
        {
            chatTextLeft.text += '\n' + "<b>" + name + "</b>: " + message;
            chatTextRight.text += '\n';
        }
        else
        {
            chatTextRight.text += '\n' + "<b>" + name + "</b>: " + message;
            chatTextLeft.text += '\n';
        }
        
    }

    // When a client hits the enter button, send the message in the InputField
    [Client]
    public void Send()
    {
        if (!Input.GetKeyDown(KeyCode.Return)) { return; }
        if (string.IsNullOrWhiteSpace(inputField.text)) { return; }
        CmdSendMessage(inputField.text, myName);
        inputField.text = string.Empty;
    }

    [Command(requiresAuthority = false)]
    private void CmdSendMessage(string message, string name)
    {
        RpcHandleMessage(message, name);
        
    }

    [ClientRpc]
    private void RpcHandleMessage(string message, string name)
    {
        if (name == myName)
            HandleNewMessage(message, name, false);
        else
            HandleNewMessage(message, name, true);
    }

}