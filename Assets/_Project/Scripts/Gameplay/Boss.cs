using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Netcode.Components;
using Netcode.Extensions;

public class Boss : NetworkBehaviour
{
    [SerializeField] private float maxHealth = 50f;
    private Slider healthSlider;
    [SerializeField] private float speed; 
    private NetworkVariable<float> currentHealth = new NetworkVariable<float>(50f);
    private Coroutine logicRoutine;
    private bool isLaserPhaseTriggered = false;
    private SpriteRenderer spriteRenderer;

    [Header("Attack")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireTime;

    [Header("Moverment")]
    private Transform[] movePointsInRight;
    private Transform[] movePointsInLeft;
    private int currentMovePoint = 1;

    public override void OnNetworkSpawn()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (GameManager.Instance != null)
        {
            movePointsInRight = GameManager.Instance.bossMovePointsInRight;
            movePointsInLeft = GameManager.Instance.bossMovePointsInLeft;
            healthSlider = GameManager.Instance.bossHealthSlider;
        }

        if (IsServer)
        {
            currentHealth.Value = maxHealth;
        }
    
        if (healthSlider != null) {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth.Value;
        }

        currentHealth.OnValueChanged += (oldValue, newValue) => {
            healthSlider.value = newValue;
            StartCoroutine(FlashRed()); 
        };

        if (IsServer)
        {
            StartCoroutine(WaitAndStartLogic());
        }
    }

    IEnumerator WaitAndStartLogic()
    {
        yield return new WaitUntil(() => GameManager.isGameStarted);
        logicRoutine = StartCoroutine(BossLogicRoutine());
    }

    IEnumerator BossLogicRoutine()
    {
        while (true)
        {
            for (int j = 0; j < 4; j++)
            {
                Shoot();

                yield return new WaitForSeconds(fireTime);
            }
            yield return StartCoroutine(ExecutePhase(movePointsInRight, true, movesBeforeTeleport: 3, shotsPerPoint: 4));

            yield return StartCoroutine(StepBack(isBossInRight: true));
            Teleport(toBehind: true);
            yield return StartCoroutine(Forward(isBossInRight: false));

            for (int j = 0; j < 3; j++)
            {
                Shoot();

                yield return new WaitForSeconds(fireTime);
            }
            yield return StartCoroutine(ExecutePhase(movePointsInLeft, false, movesBeforeTeleport: 2, shotsPerPoint: 3));

            yield return StartCoroutine(StepBack(isBossInRight: false));
            Teleport(toBehind: false);
            yield return StartCoroutine(Forward(isBossInRight: true));
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void TakeDamageServerRpc(float damage)
    {
        if (!IsServer) return;

        currentHealth.Value -= damage;
        healthSlider.value = currentHealth.Value;

        StartCoroutine(FlashRed());

        if (currentHealth.Value <= 10f && !isLaserPhaseTriggered)
        {
            SwitchToLaserPhase();
        }

        if (currentHealth.Value <= 0) Die();
    }

    private void Shoot()
    {
        if (!IsServer) return;

        NetworkObject bulletBossBasicNetObj = NetworkObjectPool.Singleton.GetNetworkObject(
            GameManager.Instance.bulletBossBasicPrefab, 
            firePoint.position, 
            firePoint.rotation
        );

        if (bulletBossBasicNetObj != null)
        {
            bulletBossBasicNetObj.Spawn();
        }
    }

    private void FireLazer()
    {
        if (!IsServer) return;

        Quaternion lazerRotation = Quaternion.Euler(0f, 0f, -180f);

        NetworkObject bulletBossLazerNetObj = NetworkObjectPool.Singleton.GetNetworkObject(
            GameManager.Instance.bulletBossLazerPrefab, 
            firePoint.position, 
            lazerRotation
        );

        if (bulletBossLazerNetObj != null)
        {
            bulletBossLazerNetObj.Spawn();
        }
    }

    private void Die()
    {
        ShowVictoryClientRpc();

        GetComponent<NetworkObject>().Despawn();
    }
    
    [ClientRpc]
    private void ShowVictoryClientRpc()
    {
        GameManager.Instance.backgroundOverlay.gameObject.SetActive(true);
        GameManager.Instance.notiContainer.gameObject.SetActive(true);
        GameManager.Instance.playAgainButton.gameObject.SetActive(true);
        GameManager.Instance.homeButton.gameObject.SetActive(true);

        Color color;
        ColorUtility.TryParseHtmlString("#4DEAFF", out color);
        GameManager.Instance.notiText.color = color;
        GameManager.Instance.notiText.gameObject.SetActive(true);
        GameManager.Instance.notiText.text = "VICTORY";

        Time.timeScale = 0f; 
    }

    private void SwitchToLaserPhase()
    {
        isLaserPhaseTriggered = true;

        if (logicRoutine != null)
        {
            StopCoroutine(logicRoutine);
        }

        StopAllCoroutines();

        StartCoroutine(FlashRed());

        fireTime = 2.5f;
        speed = 1f;
        logicRoutine = StartCoroutine(LaserPhaseLoop());
    }

    IEnumerator LaserPhaseLoop()
    {
        while (true)
        {
            int nextIndex;
            do {
                nextIndex = Random.Range(0, movePointsInRight.Length);
            } while (currentMovePoint == nextIndex);

            currentMovePoint = nextIndex;

            yield return StartCoroutine(MoveToPosition(movePointsInRight[currentMovePoint]));
            FireLazer();
            
            yield return new WaitForSeconds(fireTime);
        }
    }

    IEnumerator ExecutePhase(Transform[] points, bool isRight, int movesBeforeTeleport, int shotsPerPoint)
    {
        for (int i = 0; i < movesBeforeTeleport; i++)
        {
            if (points.Length > 1)
            {
                int nextIndex;
                do {
                    nextIndex = Random.Range(0, points.Length);
                } while (currentMovePoint == nextIndex);

                currentMovePoint = nextIndex;
            }
            else { currentMovePoint = 0; }

            yield return StartCoroutine(MoveToPosition(points[currentMovePoint]));

            for (int j = 0; j < shotsPerPoint; j++)
            {
                Shoot();
                yield return new WaitForSeconds(fireTime);
            }
        }
    }

    IEnumerator MoveToPosition(Transform movePoint)
    {
        while(Vector3.Distance(transform.position, movePoint.position) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, movePoint.position, speed * Time.deltaTime);

            yield return null;
        }

        transform.position = movePoint.position;
        yield return null;
    }

    IEnumerator StepBack(bool isBossInRight)
    {
        float targetX = isBossInRight ? 12f : -12f;
        Vector3 targetPos = new Vector3(targetX, transform.position.y, 0f);

        while (Vector3.Distance(transform.position, targetPos) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetPos;  
    }

    IEnumerator Forward(bool isBossInRight)
    {
        float targetX = isBossInRight ? 6.14f : -6.14f;
        Vector3 targetPos = new Vector3(targetX, transform.position.y, 0f);

        while (Vector3.Distance(transform.position, targetPos) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetPos;
    }

    private void Teleport(bool toBehind)
    {
        float targetX = toBehind ? -12f : 12f;
        float rotZ = toBehind ? 90f : -90f;
        Vector3 targetPosition = new Vector3(targetX, transform.position.y, 0f);
        Quaternion targetRotation = Quaternion.Euler(0, 0, rotZ);

        if (TryGetComponent<NetworkTransform>(out var networkTransform))
        {
            networkTransform.Teleport(targetPosition, targetRotation, transform.localScale);
        }
        else
        {
            transform.position = targetPosition;
            transform.rotation = targetRotation;
        }
    }

    IEnumerator FlashRed()
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
}
