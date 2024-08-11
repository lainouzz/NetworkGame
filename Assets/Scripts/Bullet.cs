using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Bullet : NetworkBehaviour
{
    public float speed = 10;
    private Vector2 dir;
    private void Start()
    {
        if (IsServer)
        {
            Invoke(nameof(DespawnBullets), 5f);
        }
    }

    public void DirectionInit(Vector2 direction)
    {
        dir = -direction.normalized;
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsServer)
        {
            Player player = other.GetComponent<Player>();
            if (player != null)
            {
                player.TakeDamageRpc(50);
                DespawnBullets();
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(dir * (speed * Time.deltaTime));
    }

    void DespawnBullets()
    {
        if (NetworkObject != null && NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn();
        }
    }
}
