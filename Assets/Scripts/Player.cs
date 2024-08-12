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
    
    [SerializeField] private float speed;

    [SerializeField] private float currentHealth;
    [SerializeField] private float maxHealth;
    [SerializeField] private float respawnTime;
    
    [SerializeField] private InputReader inputReader;
    [SerializeField] private GameObject objectToSpawn;

    private Vector3 spawnPosition;
    
    private NetworkVariable<Vector2> moveInput = new NetworkVariable<Vector2>();
    private NetworkVariable<float> playerScaleX = new NetworkVariable<float>(1f);
    public NetworkVariable<int> player1Score = new NetworkVariable<int>(0);
    public NetworkVariable<int> player2Score = new NetworkVariable<int>(0);
    
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
                HandlePlayerDeathRpc();
            }
        }
    }

    [Rpc(SendTo.Server)]
    private void HandlePlayerDeathRpc()
    {
        if (IsServer)
        {
            if (IsPlayer1())
            {
                player2Score.Value += 1;
            }
            else
            {
                player1Score.Value += 1;
            }

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
            else
            {
                transform.position = player2SpawnPos.position;
            }
            
            SetPlayerActiveRpc(true);
        }
    }
   
    private bool IsPlayer1()
    {
        return NetworkObjectId == 1;
    }

    [Rpc(SendTo.Everyone)]
    private void SetPlayerActiveRpc(bool isActive)
    {
        GetComponent<Collider2D>().enabled = isActive;
        GetComponent<SpriteRenderer>().enabled = isActive;
    }
    //ta bort senare
    private void OnGUI()
    {
        if (IsLocalPlayer)
        {
            GUI.Label(new Rect(50, 10, 500, 50), "Player 1 Score: " + player1Score.Value);
            GUI.Label(new Rect(50, 30, 500, 50), "Player 2 Score: " + player2Score.Value);
        }
    }
}
