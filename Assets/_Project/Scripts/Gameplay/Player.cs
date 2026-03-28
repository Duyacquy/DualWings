using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Netcode;

public class Player : NetworkBehaviour
{
    private NetworkVariable<float> currentHealth = new NetworkVariable<float>(3f);
    private SpriteRenderer spriteRenderer;

    [Header("I-Frames Settings")]
    [SerializeField] private float invincibilityDuration = 1.5f;
    [SerializeField] private float flashInterval = 0.1f;
    private bool isInvincible = false;

    public override void OnNetworkSpawn()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        UpdateUI(currentHealth.Value);

        currentHealth.OnValueChanged += (oldValue, newValue) =>
        {
            UpdateUI(newValue);
            if (newValue < oldValue)
            {
                StartCoroutine(FlashRed());
            }
        };
    }

    public void TakeDamage(float damage)
    {
        if (!IsServer) return;

        if (isInvincible) return;

        Player_PowerUpHandler powerUp = GetComponent<Player_PowerUpHandler>();
        if (powerUp != null && powerUp.CheckShield()) return;

        currentHealth.Value -= damage;

        if (currentHealth.Value <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(InvincibilityRoutine());
            TriggerInvincibilityVisualClientRpc();
        }
    }

    [ClientRpc]
    private void TriggerInvincibilityVisualClientRpc()
    {
        if (IsServer) return;
        StartCoroutine(InvincibilityRoutine());
    }

    private void UpdateUI(float health)
    {
        if (GameManager.Instance != null && GameManager.Instance.playerHealthText != null)
        {
            GameManager.Instance.playerHealthText.text = health.ToString();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsServer && other.CompareTag("Boss")) TakeDamage(1f);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (IsServer && other.CompareTag("Boss")) TakeDamage(1f);
    }

    private void Die()
    {
        ShowDefeatClientRpc();

        GetComponent<NetworkObject>().Despawn();
    }

    [ClientRpc]
    private void ShowDefeatClientRpc()
    {
        Time.timeScale = 0f;
        GameManager.Instance.backgroundOverlay.SetActive(true);
        GameManager.Instance.notiContainer.SetActive(true);
        GameManager.Instance.playAgainButton.SetActive(true);
        GameManager.Instance.homeButton.SetActive(true);

        Color color;
        ColorUtility.TryParseHtmlString("#FF7A3D", out color);
        GameManager.Instance.notiText.color = color;
        GameManager.Instance.notiText.text = "DEFEAT";
        GameManager.Instance.notiText.gameObject.SetActive(true);
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

    IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;
        float timer = 0;

        while (timer < invincibilityDuration)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;

            yield return new WaitForSeconds(flashInterval);
            timer += flashInterval;
        }

        spriteRenderer.enabled = true;
        isInvincible = false;
    }
}
