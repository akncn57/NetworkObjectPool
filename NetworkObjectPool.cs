using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkObjectPool : NetworkBehaviour
{
  [SerializeField] private List<PoolConfig> pools;
    
  // Maps PrefabIdHash to a queue of inactive objects
  private Dictionary<uint, Queue<NetworkObject>> poolDictionary = new Dictionary<uint, Queue<NetworkObject>>();
  // Caches prefabs by their ID for quick lookup
  private Dictionary<uint, NetworkObject> prefabDictionary = new Dictionary<uint, NetworkObject>();

  [System.Serializable]
  public struct PoolConfig
  {
      public NetworkObject prefab;
      public int initialSize;
  }
  
  public override void OnNetworkSpawn()
  {
      foreach (var config in pools)
      {
          uint globalObjectId = config.prefab.PrefabIdHash;
          prefabDictionary[globalObjectId] = config.prefab;
          poolDictionary[globalObjectId] = new Queue<NetworkObject>();

          // Pre-warm the pool by instantiating initial objects
          for (int i = 0; i < config.initialSize; i++)
          {
              NetworkObject obj = Instantiate(config.prefab, transform);
              obj.gameObject.SetActive(false);
              poolDictionary[globalObjectId].Enqueue(obj);
          }

          // Register custom spawn/despawn logic with Netcode
          NetworkManager.Singleton.PrefabHandler.AddHandler(config.prefab, new PoolPrefabInstanceHandler(config.prefab, this));
      }
  }

  /// <summary>
  /// Retrieves an object from the pool or creates a new one if empty.
  /// </summary>
  public NetworkObject GetNetworkObject(NetworkObject prefab, Vector3 pos, Quaternion rot)
  {
      uint id = prefab.PrefabIdHash;
      NetworkObject obj;

      if (poolDictionary[id].Count > 0)
      {
          // Reuse an existing inactive object
          obj = poolDictionary[id].Dequeue();
          obj.transform.position = pos;
          obj.transform.rotation = rot;
          obj.gameObject.SetActive(true);
      }
      else
      {
          // Expand pool if no objects are available
          obj = Instantiate(prefab, pos, rot);
      }

      return obj;
  }

  /// <summary>
  /// Deactivates and returns a NetworkObject to its respective pool.
  /// </summary>
  public void ReturnToPool(NetworkObject obj)
  {
      uint id = obj.PrefabIdHash;
      obj.gameObject.SetActive(false);
      poolDictionary[id].Enqueue(obj);
  }
}
