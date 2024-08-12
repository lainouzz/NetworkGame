using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using TMPro;

public class NetworkManagerUI : MonoBehaviour
{
   [SerializeField] private NetworkManager networkManager;
   
   public GameObject generalChatObj;
   public GameObject MainMenuUI;
   public GameObject ClientQuitBtn;
   
   public TMP_Text playerScore1Text;
   public TMP_Text playerScore2Text;
   
   [SerializeField] private bool isPaused;
   public void HostBtn()
   {
      if (!NetworkManager.Singleton.IsHost && !NetworkManager.Singleton.IsClient)
      {
         networkManager.StartHost();
         generalChatObj.SetActive(true);
         MainMenuUI.SetActive(false);
      }
      
   }
   public void JoinBtn()
   {
      if (!NetworkManager.Singleton.IsHost && !NetworkManager.Singleton.IsClient)
      {
         networkManager.StartClient();
         generalChatObj.SetActive(true);
         MainMenuUI.SetActive(false);
      }
   }
   public void QuitBtn()
   {
      Application.Quit();
   }

   private void Update()
   {
      if (NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsHost)
      {
         ClientQuitBtn.SetActive(true);
      }
      else
      {
         ClientQuitBtn.SetActive(false);
      }
      
     /* var player = FindObjectOfType<Player>();
      if (player != null)
      {
         player.player1Score.OnValueChanged += UpdatePlayer1Score;
         player.player2Score.OnValueChanged += UpdatePlayer2Score;
      }*/
      
      if (NetworkManager.Singleton.IsHost && Input.GetKeyDown(KeyCode.Escape))
      {
         if (isPaused)
         {
            Resume();
         }
         else
         {
            Pause();
         }
      }
   }
   
   private void Pause()
   {
      isPaused = true;
      MainMenuUI.SetActive(true);
      
      var player = GetComponent<Player>();
      if (player != null)
      {
         player.enabled = false;
      }

      ServerPauseRpc();
   }

   private void Resume()
   {
      isPaused = false;
      MainMenuUI.SetActive(false);

      var player = GetComponent<Player>();
      if (player != null)
      {
         player.enabled = true;
      }
      ServerResumeRpc();
   }
   
   [ServerRpc(RequireOwnership = false)]
   private void ServerPauseRpc(ServerRpcParams rpcParams = default)
   {
      if (NetworkManager.Singleton.IsHost)
      {
         Time.timeScale = 0;
         
         var player = GetComponent<Player>();
         if (player != null)
         {
            player.enabled = false;
            
         }
      }
   }
   
   [ServerRpc(RequireOwnership = false)]
   private void ServerResumeRpc(ServerRpcParams rpcParams = default)
   {
      if (NetworkManager.Singleton.IsHost)
      {
         Time.timeScale = 1;
         
         var player = GetComponent<Player>();
         if (player != null)
         {
            player.enabled = true;
            Time.timeScale = 1;
         }
      }
   }
   

   public void UpdatePlayer1Score(int newScore)
   {
      playerScore1Text.text = "Score: " + newScore;
   }
   public void UpdatePlayer2Score(int newScore)
   {
      playerScore2Text.text = "Score: " + newScore;
   }

   private Player FindPlayer(ulong clientId)
   {
      foreach (var player in FindObjectsOfType<Player>())
      {
         if (player.OwnerClientId == clientId)
         {
            return player;
         }
      }

      return null;
   }
}

  
