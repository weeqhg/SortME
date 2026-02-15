using System;
using Unity.Netcode;
using UnityEngine;

public class OrderManager : NetworkBehaviour
{
    [Serializable]
    public class Order
    {
        public int containerID;
        public ItemManager item;
        public float timeLimit;
        public float maxTimeLimit;
        //public float reward;
    }

    [SerializeField] private OrderUI _orderUI;
    [SerializeField] private RackManager _rackManager;
    [SerializeField] private Vector2 _orderDelayRange = new Vector2(30f, 60f);
    [SerializeField] private Vector2 _orderTimeLimitRange = new Vector2(60f, 120f);
    [SerializeField] private Vector2 _orderRewardRange = new Vector2(50f, 200f);
    [SerializeField] private ContainerOrder[] _containers;

    private Order _activeOrder;
    private float _orderTimer;
    private float _nextOrderTime;

    public void Init()
    {
        _orderUI.Init();
        _nextOrderTime = UnityEngine.Random.Range(_orderDelayRange.x, _orderDelayRange.y);
    }

    public override void OnNetworkSpawn()
    {
        for (int i = 0; i < _containers.Length; i++)
        {
            _containers[i].Open();
        }
    }

    public void CompleteOrder()
    {
        if (_activeOrder != null)
        {
            _activeOrder.item.OnOutRack -= OnItemStateChanged;
            _activeOrder.item.OnDestroyItem -= OnDestroyItem;

            _activeOrder = null;

            _orderUI.ChangeStateOnCompleteClientRpc();
        }
    }

    private void Update()
    {
        if (!IsServer) return;
        if (_rackManager == null) return;

        if (_activeOrder != null)
        {
            _activeOrder.timeLimit -= Time.deltaTime;

            float progress = _activeOrder.timeLimit / _activeOrder.maxTimeLimit; // или начальное значение timeLimit
            _orderUI.WaitTimer(progress);

            if (_activeOrder.timeLimit <= 0)
            {
                _activeOrder.item.ChangeItemState(ItemState.Stored);

                _activeOrder.item.OnOutRack -= OnItemStateChanged;
                _activeOrder.item.OnDestroyItem -= OnDestroyItem;

                _orderUI.ChangeStateOnFailClientRpc();

                _activeOrder = null;
            }

            return;
        }

        if (_activeOrder != null) return;

        _orderTimer += Time.deltaTime;

        if (_orderTimer >= _nextOrderTime)
        {
            CreateNewOrder();
            ResetOrderTimer();
        }
    }

    private void CreateNewOrder()
    {
        ItemManager newItem = _rackManager.GetRandomRackIDandID();

        if (newItem == null)
        {
            _orderUI.ChangeStateOnWaitClientRpc();
            return;
        }


        newItem.ChangeItemState(ItemState.Ordering);
        ContainerOrder randomCont = _containers.Length > 0 ? _containers[UnityEngine.Random.Range(0, _containers.Length)] : null;
        float timeLimitValue = UnityEngine.Random.Range(_orderTimeLimitRange.x, _orderTimeLimitRange.y);

        Order newOrder = new Order
        {
            containerID = randomCont.GetId(),
            item = newItem,
            timeLimit = timeLimitValue,
            maxTimeLimit = timeLimitValue
            //reward = UnityEngine.Random.Range(_orderRewardRange.x, _orderRewardRange.y),F
        };

        randomCont.ChangeOrderID(newItem.info.nameKeyItem);

        _activeOrder = newOrder;

        _activeOrder.item.OnOutRack += OnItemStateChanged;
        _activeOrder.item.OnDestroyItem += OnDestroyItem;

        _orderUI.UpdateOrder(newItem.info.nameKeyItem, newItem.info.icon);
    }


    private void OnItemStateChanged()
    {
        if (_activeOrder != null)
        {
            _orderUI.UpdateGate(_activeOrder.containerID, _activeOrder.item.info.box);
            _activeOrder.item.OnOutRack -= OnItemStateChanged;
        }
    }

    private void OnDestroyItem()
    {
        if (_activeOrder != null)
        {
            _activeOrder.item.OnOutRack -= OnItemStateChanged;
            _activeOrder.item.OnDestroyItem -= OnDestroyItem;

            _activeOrder = null;
            _orderUI.ChangeStateOnFailClientRpc();
        }
    }

    private void ResetOrderTimer()
    {
        _orderTimer = 0f;
        _nextOrderTime = UnityEngine.Random.Range(_orderDelayRange.x, _orderDelayRange.y);
    }
}
