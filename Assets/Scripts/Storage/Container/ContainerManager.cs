using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using System.Collections;

public class ContainerManager : NetworkBehaviour
{
    [Header("SFX")]
    [SerializeField] private AudioClip _bip;
    [SerializeField] private AudioClip _truckDrive;
    [SerializeField] private AudioClip[] _gateSound;

    [Header("Prefab")]
    [SerializeField] private GameObject[] _itemsPrefab;

    [Header("Reference")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private Transform[] _spawnPos;
    [SerializeField] private Transform[] _gates;
    
    [Header("Setting")]
    [SerializeField] private float _targetHeight;
    private List<GameObject> _spawnedItems = new List<GameObject>();
    private NetworkVariable<int> _netIndex = new(-1);
    private bool _isAnimating = false;
    private int _currentCount;
    public override void OnNetworkSpawn()
    {
        _netIndex.OnValueChanged += OnGateChanged;
    }

    private void OnGateChanged(int oldValue, int newValue)
    {
        if (newValue == -1) return;

        if (oldValue == -1)
        {
            OpenGate(newValue);
        }
        else
        {
            CloseThenOpen(oldValue, newValue);
        }
    }

    //Это должен выполнять только сервер
    public void SpawnItems(int count)
    {
        if (!IsServer || _itemsPrefab.Length == 0 || _spawnPos.Length == 0 || _isAnimating) return;

        _netIndex.Value = Random.Range(0, _spawnPos.Length);
        _currentCount = count;
    }


    private void CloseThenOpen(int closeIndex, int openIndex)
    {
        if (_isAnimating) return;
        _isAnimating = true;

        Sequence sequence = DOTween.Sequence();

        // Закрытие
        sequence.AppendCallback(() => PlayRandomSound(_gateSound))
                .Append(_gates[closeIndex].DOLocalMoveY(0, 1.5f).SetEase(Ease.OutCubic))
                .AppendInterval(0.5f)
                .AppendCallback(() => _audioSource.PlayOneShot(_truckDrive))
                .AppendInterval(1f);

        // Открытие с предметами
        sequence.AppendCallback(() =>
        {
            SpawnItemsInternal(_currentCount);
            _audioSource.PlayOneShot(_bip);
        })
        .AppendInterval(1f)
        .AppendCallback(() => PlayRandomSound(_gateSound))
        .Append(_gates[openIndex].DOLocalMoveY(_targetHeight, 1.5f).SetEase(Ease.OutCubic))
        .OnComplete(() => _isAnimating = false);
    }

    private void OpenGate(int index)
    {
        Sequence sequence = DOTween.Sequence();

        sequence.AppendCallback(() => _audioSource.PlayOneShot(_bip))
                .AppendInterval(2f)
                .AppendCallback(() =>
                {
                    SpawnItemsInternal(_currentCount);
                    PlayRandomSound(_gateSound);
                })
                .Append(_gates[index].DOLocalMoveY(_targetHeight, 1.5f).SetEase(Ease.OutCubic));
    }

    private void SpawnItemsInternal(int count)
    {
        if (!IsServer) return;

        Transform pos = _spawnPos[_netIndex.Value];

        for (int i = 0; i < count; i++)
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
                info.state = ItemState.Arrived;

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
                info.state = ItemState.Arrived;

                NetworkObject netObj = obj.GetComponent<NetworkObject>();
                if (netObj != null)
                    netObj.Spawn();
            }
        }
    }

    private void PlayRandomSound(AudioClip[] clips)
    {
        if (clips.Length == 0) return;
        _audioSource.PlayOneShot(clips[Random.Range(0, clips.Length)]);
    }

    public override void OnNetworkDespawn()
    {
        _netIndex.OnValueChanged -= OnGateChanged;
        DOTween.KillAll();
    }
}
