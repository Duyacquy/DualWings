using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Netcode.Extensions;

public class EnemySpawner : NetworkBehaviour
{
    [Header("Basic Enemy Settings")]
    [SerializeField] private float spawnRateEnemyBasic = 3f;
    [SerializeField] private float spawnXEnemyBasic = 12f;
    [SerializeField] private float minYEnemyBasic = -4f;
    [SerializeField] private float maxYEnemyBasic = 4f;

    [Header("X Enemy Settings")]
    [SerializeField] private float spawnRateEnemyX = 10f;
    [SerializeField] private float spawnYEnemyX = 7f;
    [SerializeField] private float minXEnemyX = -2.5f;
    [SerializeField] private float maxXEnemyX = 2.5f;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            StartCoroutine(StartSpawning());
        }
    }

    IEnumerator StartSpawning()
    {
        yield return new WaitUntil(() => GameManager.isGameStarted);

        yield return new WaitForSeconds(0.2f);

        StartCoroutine(SpawnBasicRoutine());
        StartCoroutine(SpawnXRoutine());
    }

    IEnumerator SpawnBasicRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnRateEnemyBasic);

            SpawnBasicEnemy();
        }
    }

    IEnumerator SpawnXRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnRateEnemyX);

            SpawnXEnemy();
        }
    }

    private void SpawnBasicEnemy()
    {
        float spawnY = Random.Range(minYEnemyBasic, maxYEnemyBasic);
        Vector3 spawnPos = new Vector3(spawnXEnemyBasic, spawnY, 0f);
        Quaternion spawnRot = Quaternion.Euler(0f, 0f, -90f);

        NetworkObject enemyNetObj = NetworkObjectPool.Singleton.GetNetworkObject(
            GameManager.Instance.EnemyBasicPrefab, 
            spawnPos, 
            spawnRot
        );

        if (enemyNetObj != null)
        {
            enemyNetObj.Spawn();
        }
    }

    private void SpawnXEnemy()
    {
        float spawnX = Random.Range(minXEnemyX, maxXEnemyX);
        Vector3 spawnPos = new Vector3(spawnX, spawnYEnemyX, 0f);
        Quaternion spawnRot = Quaternion.Euler(0f, 0f, 0f);

        NetworkObject enemyNetObj = NetworkObjectPool.Singleton.GetNetworkObject(
            GameManager.Instance.EnemyXPrefab, 
            spawnPos, 
            spawnRot
        );

        if (enemyNetObj != null)
        {
            enemyNetObj.Spawn();
        }
    }
}
