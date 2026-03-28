using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Enemy_basic : Enemy
{
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            health = 1f; 
        }
    }

    void Update()
    {
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer) return;

        transform.Translate(Vector3.down * speed * Time.deltaTime);

        if (transform.position.x < -12)
        {
            GetComponent<NetworkObject>().Despawn();
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (!IsServer) return;
        
        if (other.CompareTag("Player"))
        {
            Player player = other.GetComponent<Player>();

            if (player != null)
            {
                player.TakeDamage(damage);
                GetComponent<NetworkObject>().Despawn();
            }
        }
    }
}
