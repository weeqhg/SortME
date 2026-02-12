using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class OrderManager : NetworkBehaviour
{
    [System.Serializable]
    public class Order
    {
        public int containerID;
        public ItemInfo itemInfo;
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
    private List<Order> _activeOrders = new List<Order>();
    private float _orderTimer;
    private float _nextOrderTime;

    public void Init()
    {
        _orderUI.Init();
        _nextOrderTime = UnityEngine.Random.Range(_orderDelayRange.x, _orderDelayRange.y);

        for (int i = 0; i < _containers.Length; i++)
        {
            _containers[i].Open();
        }
    }

    public void CompleteOrder(string nameKeyItem)
    {
        Order order = _activeOrders.Find(o => o.itemInfo.nameKeyItem == nameKeyItem);

        if (order != null)
        {
            _activeOrders.Remove(order);

            _orderUI.ChangeStateOnWaitClientRpc();

            //Здесь уведомлять игроков о выполнении квеста
        }
    }

    private void Update()
    {
        if (!IsServer) return;
        if (_rackManager == null) return;

        if (_activeOrders.Count > 0)
        {
            Order order = _activeOrders[0];
            order.timeLimit -= Time.deltaTime;

            float progress = order.timeLimit / order.maxTimeLimit; // или начальное значение timeLimit
            _orderUI.WaitTimer(progress);

            if (order.timeLimit <= 0)
            {
                _activeOrders.RemoveAt(0);
                order.itemInfo.stateChanged -= OnItemStateChanged;
                _orderUI.ChangeStateOnWaitClientRpc();
                Debug.Log($"Заказ на {order.itemInfo.nameKeyItem} провален (время истекло)");
            }

            return;
        }

        if (_activeOrders.Count > 0) return;

        _orderTimer += Time.deltaTime;

        if (_orderTimer >= _nextOrderTime)
        {
            CreateNewOrder();
            ResetOrderTimer();
        }
    }

    private void CreateNewOrder()
    {
        ItemInfo itemInfo = _rackManager.GetRandomRackIDandID();

        if (itemInfo == null)
        {
            _orderUI.ChangeStateOnWaitClientRpc();
            Debug.Log("Нет доступных предметов для заказа");
            return;
        }

        itemInfo.stateChanged += OnItemStateChanged;
        ContainerOrder randomCont = _containers.Length > 0 ? _containers[UnityEngine.Random.Range(0, _containers.Length)] : null;
        float timeLimitValue = UnityEngine.Random.Range(_orderTimeLimitRange.x, _orderTimeLimitRange.y);

        Order newOrder = new Order
        {
            containerID = randomCont.GetId(),
            itemInfo = itemInfo,
            timeLimit = timeLimitValue,
            maxTimeLimit = timeLimitValue
            //reward = UnityEngine.Random.Range(_orderRewardRange.x, _orderRewardRange.y),F
        };

        randomCont.ChangeOrderID(itemInfo.nameKeyItem);

        _orderUI.UpdateOrder(itemInfo.nameKeyItem, itemInfo.icon);

        _activeOrders.Add(newOrder);
    }


    private void OnItemStateChanged(ItemInfo item, ItemState newState)
    {
        if (newState == ItemState.Ordering)
        {
            Order order = _activeOrders.Find(o => o.itemInfo == item);

            _orderUI.UpdateGate(order.containerID, order.itemInfo.box);

            item.stateChanged -= OnItemStateChanged;
        }
    }

    private void ResetOrderTimer()
    {
        _orderTimer = 0f;
        _nextOrderTime = UnityEngine.Random.Range(_orderDelayRange.x, _orderDelayRange.y);
    }
}
