using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class ChatMessage : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textMessage;

    public void SetText(string str)
    {
        textMessage.text = str;
    }
}
