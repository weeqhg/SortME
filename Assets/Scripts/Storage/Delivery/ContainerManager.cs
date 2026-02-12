using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine.UI;

public class ContainerManager : NetworkBehaviour
{

    [System.Serializable]
    public class ContainerData
    {
        public Transform spawnPos;
        public ContainerDeliver containerDeliver;
        [HideInInspector] public bool isScheduledForDelivery = false;
    }


    [Header("SFX")]
    [SerializeField] private AudioClip _bip;
    [SerializeField] private AudioClip _truckDrive;

    [Header("Prefab")]
    [SerializeField] private GameObject[] _itemsPrefab;

    [Header("Reference")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private ContainerData[] _containers;
    [SerializeField] private Slider _deliverySlider;

    private NetworkVariable<float> _netDeliveryProgress = new NetworkVariable<float>(0f);

    [Header("Delivery Settings")]
    [SerializeField] private Vector2 _deliveryDelayRange = new Vector2(30f, 60f);
    [SerializeField] private Vector2Int _itemCountRange = new Vector2Int(1, 5);

    [Header("Setting")]
    private List<GameObject> _spawnedItems = new List<GameObject>();


    private float _deliveryTimer;
    private float _currentDeliveryTime;
    private bool _isDelivering = false;
    private int _deliveryTargetContainer = -1;
    private int _currentCount;

    public void Init()
    {
        _deliveryTargetContainer = GetFreeContainer();
        _containers[_deliveryTargetContainer].isScheduledForDelivery = true;

        _currentDeliveryTime = 15f;
        _deliveryTimer = 0;
        _isDelivering = true;
        _deliverySlider.value = 0;

    }

    public override void OnNetworkSpawn()
    {
        _netDeliveryProgress.OnValueChanged += OnDeliveryProgressChanged;
    }

    private void Update()
    {
        if (!IsServer) return;

        if (_isDelivering)
        {
            _deliveryTimer += Time.deltaTime;
            float progress = _deliveryTimer / _currentDeliveryTime;
            _netDeliveryProgress.Value = progress;

            if (_deliveryTimer >= _currentDeliveryTime)
            {
                ExecuteDelivery();
            }
        }
    }


    private void ScheduleNextDelivery()
    {
        _deliveryTargetContainer = GetFreeContainer();

        if (_deliveryTargetContainer != -1)
        {
            _containers[_deliveryTargetContainer].isScheduledForDelivery = true;
            _currentDeliveryTime = Random.Range(_deliveryDelayRange.x, _deliveryDelayRange.y);
            _deliveryTimer = 0;
            _isDelivering = true;
            _deliverySlider.value = 0f;
        }
        else
        {
            _isDelivering = false;
            _deliverySlider.value = 1f;
        }
    }

    private void ExecuteDelivery()
    {
        if (_deliveryTargetContainer == -1) return;

        _isDelivering = false;
        int itemCount = Random.Range(_itemCountRange.x, _itemCountRange.y);

        _currentCount = itemCount;

        SpawnItemsInternal(_containers[_deliveryTargetContainer]);
        _audioSource.PlayOneShot(_bip);

        _containers[_deliveryTargetContainer].containerDeliver.Open();

        _deliverySlider.DOValue(0f, 0.5f).SetEase(Ease.OutCubic)
     .OnComplete(() => ScheduleNextDelivery());
    }



    private void OnDeliveryProgressChanged(float oldValue, float newValue)
    {
        _deliverySlider.value = newValue;
    }


    private int GetFreeContainer()
    {
        for (int i = 0; i < _containers.Length; i++)
        {
            if (_containers[i].containerDeliver.IsAvalible)
                return i;
        }
        return -1;
    }

    private void SpawnItemsInternal(ContainerData container)
    {
        if (!IsServer) return;

        Transform pos = container.spawnPos;

        for (int i = 0; i < _currentCount; i++)
        {
            GameObject itemToReuse = _spawnedItems.FirstOrDefault(item =>
            {
                ItemInfo info = item.GetComponent<ItemInfo>();
                return info != null && info.state == ItemState.Dispatched;
            });

            if (itemToReuse != null)
            {
                itemToReuse.transform.position = pos.position;
                itemToReuse.SetActive(true);
                ItemInfo info = itemToReuse.GetComponent<ItemInfo>();
                info.ChangeItemState(ItemState.Arrived);
                info.Reset();

                NetworkObject netObj = itemToReuse.GetComponent<NetworkObject>();
                if (netObj != null && !netObj.IsSpawned)
                    netObj.Spawn();
            }
            else
            {
                GameObject prefab = _itemsPrefab[Random.Range(0, _itemsPrefab.Length)];

                GameObject obj = Instantiate(prefab, pos);
                _spawnedItems.Add(obj);
                ItemInfo info = obj.GetComponent<ItemInfo>();
                info.ChangeItemState(ItemState.Arrived);

                NetworkObject netObj = obj.GetComponent<NetworkObject>();
                if (netObj != null)
                    netObj.Spawn();
            }
        }
    }

    public void ContainerEmptied(int containerIndex)
    {
        _containers[containerIndex].isScheduledForDelivery = false;

        if (!_isDelivering)
        {
            ScheduleNextDelivery();
        }
    }


    public override void OnNetworkDespawn()
    {
        _netDeliveryProgress.OnValueChanged -= OnDeliveryProgressChanged;
        DOTween.KillAll();
    }

}
