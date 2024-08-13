using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class HandlePlayerSelection : MonoBehaviour
{
   public GameObject player1;
   public GameObject player2;

   private void Start()
   {
      NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
   }

   private void OnClientConnected(ulong clientId)
   {
      if (IsServer())
      {
         HandleClientConnected(clientId);
      }
   }

   private void HandleClientConnected(ulong clientId)
   {
      if (NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject != null)
      {
         return;
      }
      GameObject playersPrefab = (clientId % 2 == 0) ? player1 : player2;
      PllayersSpawning(clientId, playersPrefab);
   }

   private void PllayersSpawning(ulong clientId, GameObject playersPrefab)
   {
      GameObject playerInst = Instantiate(playersPrefab);
      NetworkObject networkObj = playerInst.GetComponent<NetworkObject>();
      networkObj.SpawnAsPlayerObject(clientId);
   }

   private bool IsServer()
   {
      return NetworkManager.Singleton.IsServer;
   }
   
   private void OnDestroy()
   {
      if (NetworkManager.Singleton != null)
      {
         NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
      }
   }
}
