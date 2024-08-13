using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Bullet : NetworkBehaviour
{
    public float speed = 10;
    
    private bool hasHit;
    
    private Vector2 dir;
    private void Start()
    {
        if (IsServer)
        {
            Invoke(nameof(BulletDespawn), 3f);
        }
    }

    public void DirectionInit(Vector2 direction)
    {
        dir = -direction.normalized;
    }
    

    // Update is called once per frame
    void Update()
    {
        transform.Translate(dir * (speed * Time.deltaTime));

        if (IsServer)
        {
            BulletCollisionChecker();
        }else if (!hasHit)
        {
            BulletCollisionPredictor();
        }
    }

    private void BulletCollisionPredictor()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, speed * Time.deltaTime);
        if (hit.collider != null)
        {
            Player player = hit.collider.GetComponent<Player>();
            if (player != null)
            {
                hasHit = true;
                player.TakeDamageRpc(50);
            }
        }
    }


    private void BulletCollisionChecker()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, speed * Time.deltaTime);
        if (hit.collider != null)
        {
            Player player = hit.collider.GetComponent<Player>();
            if (player != null)
            {
                player.TakeDamageRpc(50);
                BulletDespawn();
            }
        }
    }

    void BulletDespawn()
    {
        if (NetworkObject != null && NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn();
        }
    }
}
