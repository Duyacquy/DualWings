using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Bullet : NetworkBehaviour
{
    [SerializeField] private float speed = 7f;
    [SerializeField] private float lifeTime = 2f;
    [SerializeField] private float baseDamage = 1f;
    private float timer;
    private NetworkVariable<float> netDamageMultiplier = new NetworkVariable<float>(1f);
    private NetworkVariable<float> netBulletScale = new NetworkVariable<float>(0.7f);

    public void SetBulletStats(float damageMult, float scale)
    {        
        netDamageMultiplier.Value = damageMult;
        netBulletScale.Value = scale;
    }

    public override void OnNetworkSpawn()
    {
        timer = lifeTime;

        netBulletScale.OnValueChanged += HandleScaleChanged;

        UpdateVisuals(netBulletScale.Value);
    }

    public override void OnNetworkDespawn()
    {
        netBulletScale.OnValueChanged -= HandleScaleChanged;
    }

    private void HandleScaleChanged(float oldVal, float newVal)
    {
        UpdateVisuals(newVal);
    }

    private void UpdateVisuals(float scale)
    {
        transform.localScale = Vector3.one * scale;
    }

    void Update()
    {
        if (!IsServer) return;

        transform.Translate(Vector3.right * speed * Time.deltaTime);

        timer -= Time.deltaTime;
        if (timer <= 0) 
        {
            GetComponent<NetworkObject>().Despawn();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsServer) return;

        float finalDamage = baseDamage * netDamageMultiplier.Value;
        
        if (other.CompareTag("Boss"))
        {
            Boss boss = other.GetComponent<Boss>();

            if (boss != null)
            {
                boss.TakeDamageServerRpc(finalDamage);
                GetComponent<NetworkObject>().Despawn();
            }
        }
        else if (other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();

            if (enemy != null)
            {
                enemy.TakeDamage(finalDamage);
                GetComponent<NetworkObject>().Despawn();
            }
        }
    }
}
