public class PoolPrefabInstanceHandler : INetworkPrefabInstanceHandler
{
    private NetworkObject _prefab;
    private NetworkObjectPool _pool;

    public PoolPrefabInstanceHandler(NetworkObject prefab, NetworkObjectPool pool)
    {
        _prefab = prefab;
        _pool = pool;
    }

    /// <summary>
    /// Called by Netcode when it needs to spawn an object.
    /// Instead of creating a new one, we fetch it from our pool.
    /// </summary>
    public NetworkObject Instantiate(ulong ownerClientId, Vector3 position, Quaternion rotation)
    {
        return _pool.GetNetworkObject(_prefab, position, rotation);
    }

    /// <summary>
    /// Called by Netcode when an object is despawned.
    /// Instead of destroying the object, we return it to our pool for reuse.
    /// </summary>
    public void Destroy(NetworkObject networkObject)
    {
        _pool.ReturnToPool(networkObject);
    }
}
