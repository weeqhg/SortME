using Unity.Netcode;
using UnityEngine;

public class StorageManager : NetworkBehaviour
{
    private RackManager _rackManager;
    private OrderManager _orderManager;
    private ContainerManager _containerManager;

    public void Init()
    {
        _rackManager = GetComponentInChildren<RackManager>();

        _containerManager = GetComponentInChildren<ContainerManager>();
        _orderManager = GetComponentInChildren<OrderManager>();
        _containerManager.Init();
        _orderManager.Init();
    }
}
