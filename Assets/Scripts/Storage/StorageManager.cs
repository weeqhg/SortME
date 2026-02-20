using Unity.Netcode;
using UnityEngine;

public class StorageManager : NetworkBehaviour
{
    private RackManager _rackManager;
    private OrderManager _orderManager;
    private ContainerManager _containerManager;
    private ScoreCounterManager _scoreCounterManager;
    private IScoreManager _scoreManager;
    public void Init(IScoreManager scoreManager)
    {
        _scoreManager = scoreManager;

        _rackManager = GetComponentInChildren<RackManager>();
        _scoreCounterManager = GetComponentInChildren<ScoreCounterManager>();
        _containerManager = GetComponentInChildren<ContainerManager>();
        _orderManager = GetComponentInChildren<OrderManager>();

        if (_scoreManager != null) _scoreCounterManager.Init(scoreManager);
        else Debug.Log("GlobalScore не установлен");

        _containerManager.Init();
        _orderManager.Init();
    }
}
