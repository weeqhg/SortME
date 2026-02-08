using Unity.Netcode;
using UnityEngine;

public class StorageManager : NetworkBehaviour
{
    [SerializeField] private RackManager _rackManager;
    [SerializeField] private ContainerManager _containerManager;

    public void Init()
    {

    }

    [ServerRpc]
    public void SpawnItemsServerRpc(int value)
    {
        _containerManager.SpawnItems(value);
    }
    
}
