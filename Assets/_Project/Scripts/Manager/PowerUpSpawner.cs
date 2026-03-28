using System.Collections;
using UnityEngine;
using Unity.Netcode;
using Netcode.Extensions;

public class PowerUpSpawner : NetworkBehaviour
{
    [SerializeField] private float spawnTime;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            StartCoroutine(WaitAndSpawnRoutine());
        }
    }

    IEnumerator WaitAndSpawnRoutine()
    {
        yield return new WaitUntil(() => GameManager.isGameStarted);
        
        while (true)
        {
            yield return new WaitForSeconds(spawnTime);

            SpawnPowerUp();
        }
    }

    private void SpawnPowerUp()
    {
        if (GameManager.Instance.powerUpPrefab == null || GameManager.Instance.powerUpPrefab.Length == 0) return;

        int randomIndex = Random.Range(0, GameManager.Instance.powerUpPrefab.Length);
        Vector3 spawnPos = new Vector3(12f, Random.Range(-4f, 4f), 0f);

        NetworkObject powerUpNetObj = NetworkObjectPool.Singleton.GetNetworkObject(
            GameManager.Instance.powerUpPrefab[randomIndex],
            spawnPos,
            Quaternion.identity
        );

        if (powerUpNetObj != null)
        {
            powerUpNetObj.Spawn();
        }
    }
}
