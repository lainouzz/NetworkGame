using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using Unity.VisualScripting;

public class ChatManager : NetworkBehaviour
{
    public static ChatManager Singleton;

    [SerializeField] private ChatMessage chatMsgPrefab;
    [SerializeField] private CanvasGroup chatContent;
    [SerializeField] private TMP_InputField chatInput;

    public string playerName;

    private void Awake()
    {
        ChatManager.Singleton = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            SendChatMessage(chatInput.text, playerName);
            chatInput.text = " ";
        }
    }

    private void SendChatMessage(string message, string fromSender = null)
    {
        if (string.IsNullOrWhiteSpace(message)) return;

        string str = fromSender + " > " + message;
        SendChatMessageServerRpc(str);
    }

    void AddMessages(string msg)
    {
        ChatMessage ChatMsg = Instantiate(chatMsgPrefab, chatContent.transform);
        ChatMsg.SetText(msg);
    }
    [ServerRpc(RequireOwnership = false)]
    private void SendChatMessageServerRpc(string msg)
    {
        ReceiveChatMessageFromClientRpc(msg);
    }

    [ClientRpc]
    private void ReceiveChatMessageFromClientRpc(string msg)
    {
        ChatManager.Singleton.AddMessages(msg);
    }
}
