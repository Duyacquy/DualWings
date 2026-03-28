using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Enemy : NetworkBehaviour
{
    [SerializeField] protected float health;
    [SerializeField] protected float speed;
    [SerializeField] protected float damage;

    public virtual void TakeDamage(float damage)
    {
        if (!IsServer)
        {
            TakeDamageServerRpc(damage);
        }
        else
        {
            ProcessDamage(damage);
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void TakeDamageServerRpc(float damage)
    {
        ProcessDamage(damage);
    }

    protected void ProcessDamage(float damage)
    {
        health -= damage;
        if (health <= 0) Die();
    }

    protected virtual void Die()
    {
        GetComponent<NetworkObject>().Despawn();
    }
}
