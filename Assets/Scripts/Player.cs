using System.Collections;
using System.Collections.Generic;
using TMPro.Examples;
using UnityEngine;
using Unity.Netcode;
using Unity.VisualScripting;

public class Player : NetworkBehaviour
{
    public Transform spawnBulletPos;

    public Transform player1SpawnPos;
    public Transform player2SpawnPos;
    
    [SerializeField]private float speed;

    [SerializeField] private float currentHealth;
    [SerializeField] private float maxHealth;
    [SerializeField] private float respawnTime;
    
    [SerializeField] private InputReader inputReader;
    [SerializeField] private GameObject objectToSpawn;

    private Vector3 spawnPosition;
    
    private NetworkVariable<Vector2> moveInput = new NetworkVariable<Vector2>();
    private NetworkVariable<float> playerScaleX = new NetworkVariable<float>(1f);
    
    public NetworkVariable<int> player1Score = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone);
    public NetworkVariable<int> player2Score = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone);
    public NetworkVariable<int> playerId = new NetworkVariable<int>();
    
    void Start()
    {
        if (inputReader != null && IsLocalPlayer)
        {
            inputReader.MoveEvent += OnMove;
            inputReader.ShootEvent += Shoot;
        }

        if (IsServer)
        {
            spawnPosition = transform.position;
            currentHealth = maxHealth;
        }
    }

    private void OnMove(Vector2 input)
    {
        MoveRPC(input);
    }
    
    // Update is called once per frame
    void Update()
    {
        transform.localScale = new Vector3(playerScaleX.Value, transform.localScale.y, transform.localScale.z);
        
        if (IsServer)
        {
            HandleMovementAndFlipPlayerRpc();
           
        }
    }

    [Rpc(SendTo.Server)]
    private void HandleMovementAndFlipPlayerRpc()
    {
        Vector3 playerMove = (Vector3)moveInput.Value * (speed * Time.deltaTime);
        transform.position += playerMove;

        if (moveInput.Value.x < 0)
        {
            playerScaleX.Value = -Mathf.Abs(playerScaleX.Value);
        }else if (moveInput.Value.x > 0)
        {
            playerScaleX.Value = Mathf.Abs(playerScaleX.Value);
        }
    }
    
    private void Shoot()
    {
       SpawnRPC();
    }
    
    [Rpc(SendTo.Server)]
    private void SpawnRPC()
    {
        Vector2 direction = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        
        GameObject bulletInstance = Instantiate(objectToSpawn, spawnBulletPos.position, Quaternion.identity);
        NetworkObject networkObj = bulletInstance.GetComponent<NetworkObject>();
        networkObj.Spawn();

        Bullet bulletScript = bulletInstance.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.DirectionInit(direction);
        }
    }
    
    [Rpc(SendTo.Server)]
    private void MoveRPC(Vector2 data)
    {
        moveInput.Value = data;
    }

    [Rpc(SendTo.Server)]
    public void TakeDamageRpc(float damage)
    {
        if (IsServer)
        {
            currentHealth -= damage;
            if (currentHealth <= 0)
            {
                updateScore();
                HandlePlayerDeathServerRpc();
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void HandlePlayerDeathServerRpc()
    {
        if (IsServer)
        {
            SetPlayerActiveRpc(false);
            Invoke(nameof(RespawnPlayerRpc), respawnTime);
        }
    }
    
    private void RespawnPlayerRpc()
    {
        if (IsServer)
        {
            currentHealth = maxHealth;
            if (IsPlayer1())
            {
                transform.position = player1SpawnPos.position;
            }
            else if(IsPlayer2())
            {
                transform.position = player2SpawnPos.position;
            }
            
            SetPlayerActiveRpc(true);
        }
    }
   
    public bool IsPlayer1()
    {
        return playerId.Value == 1;
    }
    public bool IsPlayer2()
    {
        return playerId.Value == 2;
    }
    
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            if (NetworkManager.Singleton.ConnectedClients.Count == 1)
            {
                playerId.Value = 1;
            }
            if (NetworkManager.Singleton.ConnectedClients.Count == 2)
            {
                playerId.Value = 2;
            }
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    void UpdatePlayer1ScoreServerRpc(int newScore)
    {
        player1Score.Value = newScore;
        UpdatePlayer1ScoreClientRpc(newScore);
    }

    // Call this on the server to update Player 2's score
    [ServerRpc(RequireOwnership = false)]
    void UpdatePlayer2ScoreServerRpc(int newScore)
    {
        player2Score.Value = newScore;
        UpdatePlayer2ScoreClientRpc(newScore);
    }

    [ClientRpc]
    void UpdatePlayer1ScoreClientRpc(int newScore)
    {
        var networkUI = FindObjectOfType<NetworkManagerUI>();
        if (networkUI != null)
        {
            networkUI.UpdatePlayer1Score(newScore);
        }
    }

    // This ClientRpc updates the UI on all clients for Player 2
    [ClientRpc]
    void UpdatePlayer2ScoreClientRpc(int newScore)
    {
        var networkUI = FindObjectOfType<NetworkManagerUI>();
        if (networkUI != null)
        {
            networkUI.UpdatePlayer2Score(newScore);
        }
    }

    void updateScore()
    {
        if (IsServer)
        {
            if (IsPlayer1())
            {
                UpdatePlayer1ScoreServerRpc(player1Score.Value + 1);
            }
            else
            {
                UpdatePlayer2ScoreServerRpc(player2Score.Value + 1);
            }
        }
        
    }
    
    [Rpc(SendTo.Everyone)]
    private void SetPlayerActiveRpc(bool isActive)
    {
        GetComponent<Collider2D>().enabled = isActive;
        GetComponent<SpriteRenderer>().enabled = isActive;
    }
   
}
