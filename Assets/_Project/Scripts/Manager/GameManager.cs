using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Netcode;
using Netcode.Extensions;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;
    private NetworkVariable<bool> _isGameStartedNet = new NetworkVariable<bool>(false);
    public static bool isGameStarted => Instance != null && Instance._isGameStartedNet.Value;

    [Header("Prefabs & Points")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform spawnPoint;
    public GameObject bulletPrefab;
    public GameObject bulletBossBasicPrefab;
    public GameObject bulletBossLazerPrefab;
    public GameObject bulletEnemyXPrefab;
    public GameObject EnemyBasicPrefab;
    public GameObject EnemyXPrefab;
    public GameObject[] powerUpPrefab;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI countdownText;
    public GameObject backgroundOverlay;
    public GameObject notiContainer;
    public TextMeshProUGUI notiText;
    public GameObject playAgainButton;
    public GameObject homeButton;
    public Slider bossHealthSlider;
    public TextMeshProUGUI playerHealthText;

    [Header("Boss Settings")]
    [SerializeField] private GameObject bossPrefab;
    [SerializeField] private Transform bossSpawnPoint;
    public Transform[] bossMovePointsInRight;
    public Transform[] bossMovePointsInLeft;

    void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        Time.timeScale = 1f;

        if (IsServer)
        {
            _isGameStartedNet.Value = false;

            SpawnSharedPlayer();

            StartCoroutine(StartCountdownRoutine());
        }
    }

    private void SpawnSharedPlayer()
    {
        GameObject oldPlayer = GameObject.FindGameObjectWithTag("Player");

        if (oldPlayer != null)
        {
            var netObj = oldPlayer.GetComponent<NetworkObject>();
            if (netObj != null && netObj.IsSpawned) netObj.Despawn();
        }

        GameObject go = Instantiate(playerPrefab, spawnPoint.position, Quaternion.identity);

        go.GetComponent<NetworkObject>().Spawn();
    }

    private void SpawnBoss()
    {
        Quaternion rot = Quaternion.Euler(0, 0, -90);

        NetworkObject bossNetObj = Netcode.Extensions.NetworkObjectPool.Singleton.GetNetworkObject(
            bossPrefab,
            bossSpawnPoint.position,
            rot
        );

        if (bossNetObj != null)
        {
            bossNetObj.Spawn();
        }
    }

    IEnumerator StartCountdownRoutine()
    {
        UpdateCountdownUIClientRpc("3", true);
        yield return new WaitForSeconds(1f);

        UpdateCountdownUIClientRpc("2", true);
        yield return new WaitForSeconds(1f);

        UpdateCountdownUIClientRpc("1", true);
        yield return new WaitForSeconds(1f);

        UpdateCountdownUIClientRpc("Start", true);
        yield return new WaitForSeconds(0.5f);

        _isGameStartedNet.Value = true;

        SpawnBoss();

        UpdateCountdownUIClientRpc("", false);
    }

    [ClientRpc]
    private void UpdateCountdownUIClientRpc(string message, bool isActive)
    {
        if (countdownText != null)
        {
            countdownText.text = message;
            countdownText.gameObject.SetActive(isActive);
        }

        if (backgroundOverlay != null)
        {
            backgroundOverlay.SetActive(isActive);
        }
    }
    
    [ContextMenu("Cleanup Game")]
    public void CleanupGameBeforeRestart()
    {
        if (!IsServer) return;

        NetworkObject[] allNetworkObjects = FindObjectsByType<NetworkObject>();

        foreach (var netObj in allNetworkObjects)
        {
            if (netObj.gameObject == gameObject || netObj.GetComponent<NetworkManager>() != null || netObj.GetComponent<NetworkObjectPool>() != null)
                continue;

            if (netObj.IsSpawned)
            {
                netObj.Despawn();
            }
        }
    }
}
