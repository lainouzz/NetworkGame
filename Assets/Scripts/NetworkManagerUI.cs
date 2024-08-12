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
   public GameObject MainMenuUI;
   
   [SerializeField] private bool isHosting;
   [SerializeField] private bool isJoining;
   [SerializeField] private bool isPaused;


   public void HostBtn()
   {
      if (!isHosting)
      {
         isHosting = true;
         networkManager.StartHost();
         generalChatObj.SetActive(true);
      }
   }
   public void JoinBtn()
   {
      if (!isJoining)
      {
         isJoining = true;
         networkManager.StartClient();
         generalChatObj.SetActive(true);
      }
   }
   public void QuitBtn()
   {
      Application.Quit();
   }

   private void Update()
   {
      if (isHosting)
      {
         MainMenuUI.SetActive(false);
         
      }else if (isJoining)
      {
         MainMenuUI.SetActive(false);

      }
      handlePauseRpc();
      
   }
   [Rpc(SendTo.Everyone)]
   void handlePauseRpc()
   {
      if (Input.GetKeyDown(KeyCode.Escape))
      {
         if (isPaused)
         {
            ResumeRpc();
         }
         else
         {
           PauseRpc();
         }
      }
   }
   [Rpc(SendTo.Everyone)]
   private void PauseRpc()
   {
      isHosting = false;
      isJoining = false;
      isPaused = true;
      MainMenuUI.SetActive(true);
      Time.timeScale = 0;
   }
   [Rpc(SendTo.Everyone)]
   private void ResumeRpc()
   {
      isHosting = true;
      isJoining = true;
      isPaused = false;
      MainMenuUI.SetActive(false);
      Time.timeScale = 1;
   }
}

  
