using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Netcode.Extensions;

public class Enemy_X : Enemy
{
    private float spinDuration = 2.0f;
    private SpriteRenderer spriteRenderer;

    [Header("Attack")]
    [SerializeField] private Transform firePoint;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            health = 2f; 
            
            StopAllCoroutines(); 
            StartCoroutine(EnemyXRoutine());
        }
    }

    IEnumerator EnemyXRoutine()
    {
        Vector3 movePoint = new Vector3(transform.position.x, -0.3f, 0);

        while (Vector3.Distance(transform.position, movePoint) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, movePoint, speed * Time.deltaTime);

            yield return null;
        }

        float elapsed = 0f;
        float startRotation = transform.eulerAngles.z;
        float targetRotation = startRotation - 360f;
        float nextShotAngle = startRotation - 60f;

        while (elapsed <= spinDuration)
        {
            elapsed += Time.deltaTime;

            float currentZ = Mathf.Lerp(startRotation, targetRotation, elapsed / spinDuration);
            transform.eulerAngles = new Vector3(0, -0.3f, currentZ);
            
            if (currentZ <= nextShotAngle)
            {
                Shoot();
                nextShotAngle -= 60f; 
            }

            yield return null;
        }

        transform.eulerAngles = new Vector3(0, 0, targetRotation);

        movePoint = new Vector3(transform.position.x, -6f, 0);

        while (Vector3.Distance(transform.position, movePoint) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, movePoint, speed * Time.deltaTime);

            yield return null;
        }

        GetComponent<NetworkObject>().Despawn();
    }

    private void Shoot()
    {
        if (!IsServer) return;

        NetworkObject bulletEnemyXNetObj = NetworkObjectPool.Singleton.GetNetworkObject(
            GameManager.Instance.bulletEnemyXPrefab, 
            firePoint.position, 
            firePoint.rotation
        );

        if (bulletEnemyXNetObj != null)
        {
            bulletEnemyXNetObj.Spawn();
        }
    }

    public override void TakeDamage(float damageAmount)
    {
        FlashRedClientRpc();

        base.TakeDamage(damageAmount);
    }

    [ClientRpc]
    private void FlashRedClientRpc()
    {
        StartCoroutine(FlashRedRoutine());
    }

    IEnumerator FlashRedRoutine()
    {
        float duration = 0.2f;
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = Mathf.PingPong(timer * 2f, 1f);
            spriteRenderer.color = Color.Lerp(Color.white, Color.red, t);
            yield return null;
        }
        spriteRenderer.color = Color.white;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsServer) return;
        
        if (other.CompareTag("Player"))
        {
            Player player = other.GetComponent<Player>();

            if (player != null)
            {
                player.TakeDamage(damage);
                TakeDamage(1f);
            }
        }
    }
}
