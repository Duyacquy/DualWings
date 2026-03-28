using System.Collections;
using UnityEngine;
using Unity.Netcode;

public class Player_PowerUpHandler : NetworkBehaviour
{
    [Header("Visual References")]
    [SerializeField] private GameObject shieldVisual;

    [Header("Network State")]
    private NetworkVariable<bool> isShieldActive = new NetworkVariable<bool>(false);
    private float originalMoveSpeed;
    private float originalShootTime;
    private float originalChargeSliderMax;

    private Player_control planeMoveControl;
    private Player_shoot planeShootControl;

    [HideInInspector] public float currentDamageMultiplier = 1f;
    [HideInInspector] public float currentBulletScale = 0.7f;

    private void Awake()
    {
        planeMoveControl = GetComponent<Player_control>();
        planeShootControl = GetComponent<Player_shoot>();
    }

    void Start()
    {
        if (planeMoveControl != null) originalMoveSpeed = planeMoveControl.speed;
        if (planeShootControl != null)
        {
            originalShootTime = planeShootControl.chargeTargetTimeNet.Value;
            if (planeShootControl.chargeSlider != null)
            {
                originalChargeSliderMax = planeShootControl.chargeSlider.maxValue;
            }
        }
    }

    public override void OnNetworkSpawn()
    {
        isShieldActive.OnValueChanged += (oldVal, newVal) => {
            if (shieldVisual != null) shieldVisual.SetActive(newVal);
        };

        if (shieldVisual != null) shieldVisual.SetActive(isShieldActive.Value);
    }

    public override void OnNetworkDespawn()
    {
        isShieldActive.OnValueChanged -= OnShieldChanged;

        StopAllCoroutines();

        if (IsServer)
        {
            ResetStats();
        }
    }

    private void OnShieldChanged(bool oldVal, bool newVal)
    {
        if (shieldVisual != null) shieldVisual.SetActive(newVal);
    }

    private void ResetStats()
    {
        if (planeMoveControl != null) planeMoveControl.speed = originalMoveSpeed;
        if (planeShootControl != null) planeShootControl.chargeTargetTimeNet.Value = originalShootTime;
        
        currentDamageMultiplier = 1f;
        currentBulletScale = 0.7f;
        isShieldActive.Value = false;
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void RequestPowerUpServerRpc(PowerUpType type)
    {
        if (!IsServer) return;

        StopCoroutine(type.ToString() + "Routine");

        switch (type)
        {
            case PowerUpType.Speed:
                StartCoroutine(SpeedRoutine());
                break;
            case PowerUpType.Damage:
                StartCoroutine(DamageRoutine());
                break;
            case PowerUpType.Shield:
                StartCoroutine(ShieldRoutine());
                break;
        }
    }

    IEnumerator SpeedRoutine()
    {
        if (planeMoveControl) planeMoveControl.speed = originalMoveSpeed * 1.5f;

        if (planeShootControl)
        {
            planeShootControl.chargeTargetTimeNet.Value = originalShootTime * 0.5f;
        }

        yield return new WaitForSeconds(10f);

        if (planeMoveControl) planeMoveControl.speed = originalMoveSpeed;
        if (planeShootControl)
        {
            planeShootControl.chargeTargetTimeNet.Value = originalShootTime;
        }
    }

    IEnumerator DamageRoutine()
    {
        currentDamageMultiplier = 2f;
        currentBulletScale = 2f;

        yield return new WaitForSeconds(12f);

        currentDamageMultiplier = 1f;
        currentBulletScale = 0.7f;
    }

    IEnumerator ShieldRoutine()
    {
        isShieldActive.Value = true;

        yield return new WaitForSeconds(18f);

        isShieldActive.Value = false;
    }

    public bool CheckShield()
    {
        if (!IsServer) return false;

        if (isShieldActive.Value)
        {
            isShieldActive.Value = false;
            return true; 
        }
        return false;
    }
}