using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public enum PowerUpType { Speed, Damage, Shield }

public class Power_up_item : NetworkBehaviour
{
    public PowerUpType type;
    [SerializeField] private float scrollSpeed = 2f;
    private KeyCode collectKey;

    void Start()
    {
        switch (type)
        {
            case PowerUpType.Speed: collectKey = KeyCode.S; break;
            case PowerUpType.Damage: collectKey = KeyCode.P; break;
            case PowerUpType.Shield: collectKey = KeyCode.U; break;
        }
    }

    void Update()
    {
        transform.Translate(Vector3.left * scrollSpeed * Time.deltaTime);
        
        if (IsServer)
        {
            if (transform.position.x < -12f)
            {
                GetComponent<NetworkObject>().Despawn();
            }
        }

        if (IsClient && !IsServer)
        {
            if (Input.GetKeyDown(collectKey))
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    float distance = Vector3.Distance(transform.position, player.transform.position);
                    if (distance < 1f)
                    {
                        CollectItemServerRpc();
                    }
                }
            }
        }
        
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void CollectItemServerRpc(RpcParams rpcParams = default)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.GetComponent<Player_PowerUpHandler>().RequestPowerUpServerRpc(type);

            GetComponent<NetworkObject>().Despawn();
        }
    }
}
