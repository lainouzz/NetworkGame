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
    
    void Start()
    {
        if (inputReader != null && IsLocalPlayer)
        {
            inputReader.MoveEvent += OnMove;
            inputReader.ShootEvent += SpawnRPC;
        }
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
            transform.position += (Vector3)moveInput.Value * (speed * Time.deltaTime);
        }
    }

    [Rpc(SendTo.Server)]
    private void SpawnRPC()
    {
        NetworkObject ob = Instantiate(objectToSpawn, transform.position, transform.rotation).GetComponent<NetworkObject>();
        ob.Spawn();
    }
    
    [Rpc(SendTo.Server)]
    private void MoveRPC(Vector2 data)
    {
        moveInput.Value = data;
    }
}
