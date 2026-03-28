using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ObjectPooler : NetworkBehaviour
{
    public static ObjectPooler Instance;
    private Dictionary<GameObject, List<GameObject>> objectPools = new Dictionary<GameObject, List<GameObject>>();

    void Awake()
    {
        Instance = this;
    }

    public void CreatePool(GameObject prefab, int poolSize)
    {
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer) 
        {
            return;
        }

        if (!objectPools.ContainsKey(prefab))
            objectPools[prefab] = new List<GameObject>();

        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(prefab);

            NetworkObject netObj = obj.GetComponent<NetworkObject>();
            
            if (netObj != null)
            {
                netObj.Spawn();
            }

            obj.SetActive(false);
            objectPools[prefab].Add(obj);
        }
    }

    public GameObject GetPoolerObject(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (!IsServer) return null;

        if (!objectPools.ContainsKey(prefab)) 
        {
            Debug.LogWarning($"[Pooler] {prefab.name} chua duoc khoi tao. Dang tao moi tu dong.");
            return CreateSingleExtra(prefab, position, rotation);
        }

        foreach (GameObject obj in objectPools[prefab])
        {
            if (obj != null && !obj.activeInHierarchy) 
            {
                obj.transform.position = position;
                obj.transform.rotation = rotation;
                obj.SetActive(true);
                return obj;
            }
        }

        return CreateSingleExtra(prefab, position, rotation);
    }
    
    private GameObject CreateSingleExtra(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (!objectPools.ContainsKey(prefab))
        {
            objectPools[prefab] = new List<GameObject>();
        }

        GameObject obj = Instantiate(prefab, position, rotation);
        NetworkObject netObj = obj.GetComponent<NetworkObject>();
        
        if (netObj != null) netObj.Spawn();
        
        obj.SetActive(true); 
        objectPools[prefab].Add(obj);
        return obj;
    }
}
