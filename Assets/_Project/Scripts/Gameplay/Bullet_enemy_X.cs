using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Bullet_enemy_X : NetworkBehaviour
{
    [SerializeField] private float speed = 3f;
    [SerializeField] private float lifeTime = 5f;
    [SerializeField] private float damage = 1f;
    private float timer;

    void OnEnable()
    {
        timer = lifeTime;
    }

    void Update()
    {
        if (!IsServer) return;

        transform.Translate(Vector3.up * speed * Time.deltaTime);

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
                GetComponent<NetworkObject>().Despawn();
            }
        }
    }
}
