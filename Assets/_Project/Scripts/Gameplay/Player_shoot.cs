using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Netcode.Extensions;

public class Player_shoot : NetworkBehaviour
{
    [SerializeField] private Transform firePoint;
    public NetworkVariable<float> chargeTargetTimeNet = new NetworkVariable<float>(1f);
    public Slider chargeSlider;
    private float currentChargeTime;
    private bool isCharging = false;

    private Player_PowerUpHandler powerUpHandler;

    private void Awake()
    {
        powerUpHandler = GetComponent<Player_PowerUpHandler>();
    }
    
    public override void OnNetworkSpawn()
    {
        chargeTargetTimeNet.OnValueChanged += (oldVal, newVal) => {
            if (chargeSlider != null) chargeSlider.maxValue = newVal;
        };

        if (chargeSlider != null) chargeSlider.maxValue = chargeTargetTimeNet.Value;
    }

    void Update()
    {
        if (!IsClient || IsHost) return;

        if (!GameManager.isGameStarted) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            isCharging = true;
            currentChargeTime = 0f;
        }

        if (Input.GetKey(KeyCode.Space))
        {
            currentChargeTime += Time.deltaTime;

            if (chargeSlider != null)
            {
                chargeSlider.value = Mathf.Min(currentChargeTime, chargeTargetTimeNet.Value);            
            }
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            if (currentChargeTime >= chargeTargetTimeNet.Value && isCharging)
            {
                RequestShootServerRpc();
            }

            isCharging = false;
            currentChargeTime = 0f;
            chargeSlider.value = 0f;
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)] 
    private void RequestShootServerRpc()
    {
        NetworkObject bulletNetObj = NetworkObjectPool.Singleton.GetNetworkObject(
            GameManager.Instance.bulletPrefab, 
            firePoint.position, 
            firePoint.rotation
        );

        if (bulletNetObj.TryGetComponent<Bullet>(out var bulletScript))
        {
            bulletScript.SetBulletStats(powerUpHandler.currentDamageMultiplier, powerUpHandler.currentBulletScale);
        }

        bulletNetObj.Spawn();
    }
}
