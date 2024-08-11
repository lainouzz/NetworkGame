using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class NetworkManagerUI : MonoBehaviour
{
   [SerializeField] private NetworkManager networkManager;

   public GameObject generalChatObj;
   private void OnGUI()
   {
      if (GUILayout.Button("Host"))
      {
         networkManager.StartHost();
         generalChatObj.SetActive(true);
      }
      
      if (GUILayout.Button("Join"))
      {
         networkManager.StartClient();
         generalChatObj.SetActive(true);
      }
      
      if (GUILayout.Button("Quit"))
      {
         Application.Quit();
      }
      
   }

}
