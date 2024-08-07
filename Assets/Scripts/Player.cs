using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.VisualScripting;

public class Player : NetworkBehaviour
{
    [SerializeField] private float speed;
    
    [SerializeField] private InputReader inputReader;
    [SerializeField] private GameObject objectToSpawn;
    
    
    private NetworkVariable<Vector2> moveInput = new NetworkVariable<Vector2>();
    private Vector3 playerScale;
    void Start()
    {
        if (inputReader != null && IsLocalPlayer)
        {
            inputReader.MoveEvent += OnMove;
            inputReader.ShootEvent += Shoot;
        }

        playerScale = transform.localScale;
    }

    private void OnMove(Vector2 input)
    {
        MoveRPC(input);
    }
    
    // Update is called once per frame
    void Update()
    {
        if (IsServer)
        {
            Vector3 playerMove = (Vector3)moveInput.Value * (speed * Time.deltaTime);
            transform.position += playerMove;

            if (moveInput.Value.x < 0)
            {
                transform.localScale = new Vector3(-Mathf.Abs(playerScale.x), playerScale.y, playerScale.z);
            }else if (moveInput.Value.x > 0)
            {
                transform.localScale = new Vector3(Mathf.Abs(playerScale.x), playerScale.y, playerScale.z);
            }
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
        
        GameObject bulletInstance = Instantiate(objectToSpawn, transform.position, Quaternion.identity);
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
}
