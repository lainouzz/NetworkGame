using System.Collections;
using System.Collections.Generic;
using TMPro.Examples;
using UnityEngine;
using Unity.Netcode;
using Unity.VisualScripting;
using TMPro;
using Unity.Mathematics;

public class Player : NetworkBehaviour
{
    public Transform spawnBulletPos;
    public Transform player1SpawnPos;
    public Transform player2SpawnPos;

    public AudioClip audio;
    public AudioSource audioSource;
    
    [SerializeField]private float speed;
    
    public float maxHealth;
    [SerializeField] private float respawnTime;
    
    [SerializeField] private InputReader inputReader;
    [SerializeField] private GameObject objectToSpawn;

    private Vector3 spawnPosition;
    
    private NetworkVariable<Vector2> moveInput = new NetworkVariable<Vector2>();
    private NetworkVariable<float> playerScaleX = new NetworkVariable<float>(1f);
    
    public NetworkVariable<int> player1Score = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone);
    public NetworkVariable<int> player2Score = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone);
    public NetworkVariable<int> playerId = new NetworkVariable<int>();
    public NetworkVariable<float> currentHealth = new NetworkVariable<float>(0f);
    
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
            currentHealth.Value = maxHealth;
        }

        currentHealth.OnValueChanged += UpdateHealthUI;
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
        }
        else if (moveInput.Value.x > 0)
        {
            playerScaleX.Value = Mathf.Abs(playerScaleX.Value);
        }
    }
    
    private void Shoot()
    { 
        PlaySoundsClientRpc();
        
       SpawnRPC();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void PlaySoundsClientRpc()
    {
        audioSource.PlayOneShot(audio);
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
            currentHealth.Value -= damage;
            if (currentHealth.Value <= 0)
            {
                UpdateScore();
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
            currentHealth.Value = maxHealth;

            transform.position = IsPlayer1() ? player1SpawnPos.position : player2SpawnPos.position;
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
    void UpdatePlayerScoreServerRpc(int playerId, int newScore)
    {
        if (playerId == 1)
        {
            player1Score.Value = newScore;
        }else if (playerId == 2)
        {
            player2Score.Value = newScore;
        }
        UpdatePlayerScoreClientRpc(playerId, newScore);
    }

    [ClientRpc]
    void UpdatePlayerScoreClientRpc(int playerId, int newScore)
    {
        var networkUI = FindObjectOfType<NetworkManagerUI>();
        if (networkUI != null)
        {
            if (playerId == 1)
            {
                networkUI.UpdatePlayer1Score(newScore);
            }else if (playerId == 2)
            {
                networkUI.UpdatePlayer2Score(newScore);
            }
        }
    }

    void UpdateScore()
    {
        if (IsServer)
        {
            int Score = IsPlayer1() ? player1Score.Value + 1 : player2Score.Value + 1;
            UpdatePlayerScoreServerRpc(playerId.Value, Score);
        }
    }

    void UpdateHealthUI(float oldDamage, float damage)
    {
        var networkUI = FindObjectOfType<NetworkManagerUI>();
        if (networkUI != null)
        {
            if (IsPlayer1())
            {
                networkUI.UpdatePlayer1Health(damage);
            }else if (IsPlayer2())
            {
                networkUI.UpdatePlayer2Health(damage);
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
