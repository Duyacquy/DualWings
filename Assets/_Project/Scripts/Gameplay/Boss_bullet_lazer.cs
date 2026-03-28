using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Boss_bullet_lazer : NetworkBehaviour
{
    [SerializeField] private float lifeTime;
    [SerializeField] private float damage;
    private float timer;

    void OnEnable()
    {
        timer = lifeTime;
    }

    void Update()
    {
        if (!IsServer) return;

        timer -= Time.deltaTime;
        if (timer <= 0) 
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
            }
        }
    }
}
