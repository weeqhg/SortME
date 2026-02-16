using Unity.Netcode;
using UnityEngine;

public class StorageManager : NetworkBehaviour
{
    private RackManager _rackManager;
    private OrderManager _orderManager;
    private ContainerManager _containerManager;
    private ScoreCounterManager _scoreCounterManager;
    private IGlobalScoreManager _globalScore;
    public void Init(IGlobalScoreManager globalScore)
    {
        _globalScore = globalScore;

        _rackManager = GetComponentInChildren<RackManager>();
        _scoreCounterManager = GetComponentInChildren<ScoreCounterManager>();
        _containerManager = GetComponentInChildren<ContainerManager>();
        _orderManager = GetComponentInChildren<OrderManager>();

        if (_globalScore != null) _scoreCounterManager.Init(_globalScore);
        else Debug.Log("GlobalScore не установлен");

        _containerManager.Init();
        _orderManager.Init();
    }
}
