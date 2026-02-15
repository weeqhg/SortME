using UnityEngine;
using DG.Tweening;
using Unity.Netcode;
using System.Collections;
using System;
public enum ItemType
{
    Box1x1,
    Box1x2,
    Box0x4
}

[RequireComponent(typeof(Rigidbody))]
public class ItemManager : NetworkBehaviour
{
    [Header("Звуки")]
    [SerializeField] private AudioClip[] _clipsDrop;
    private AudioSource _audioSource;

    [Header("Анимация удара")]
    [SerializeField] private float damageAnimationDuration = 0.3f;
    [SerializeField] private float scalePunch = 0.1f;

    [Header("Предметы")]
    [SerializeField] private ItemType _itemType;

    [SerializeField] private int _currentDurabilyItem = 100;
    [SerializeField] private int _currentDurabilyBox = 100;
    [SerializeField] private Mesh _cartonBox;
    private ItemScriptableObject[] _items;
    private Color _originalColor = Color.black;
    private Vector3 _originalScale;
    private Material _itemMaterial;
    private MeshFilter _meshFilter;
    private ParticleSystem _particalSystem;
    private Sequence _damageSequence;
    private NetworkVariable<int> _netDurability = new NetworkVariable<int>(100);
    private NetworkVariable<bool> _isPacked = new NetworkVariable<bool>(true);
    private Mesh _currentMesh;
    private Texture _currentTexture;
    private MeshRenderer _meshRenderer;
    private MaterialPropertyBlock _propertyBlock;

    public GameObject GameObject => gameObject;
    public event Action OnOutRack;
    public event Action OnDestroyItem;
    public ItemInfo info;

    private float _lastDamageTime;
    private float _damageCooldown = 0.5f;
    public void OutRack() 
    {
        OnOutRack?.Invoke();
    }

    public override void OnNetworkSpawn()
    {
        _netDurability.OnValueChanged += OnDurabilityChanged;
        _isPacked.OnValueChanged += OnPackedStateChanged;
        Init();
    }

    private void Init()
    {
        _audioSource = GetComponentInChildren<AudioSource>();
        info = GetComponent<ItemInfo>();
        _particalSystem = GetComponentInChildren<ParticleSystem>();
        _meshFilter = GetComponent<MeshFilter>();

        _meshRenderer = GetComponent<MeshRenderer>();

        _propertyBlock = new MaterialPropertyBlock();

        if (_meshRenderer != null)
        {
            _itemMaterial = _meshRenderer.material;
            _originalColor = Color.black;
        }
        _originalScale = transform.localScale;

        InitArrayItems();

        if (IsServer) Reset();
    }

    public void ChangeItemState(ItemState newState)
    {
        info.ChangeItemState(newState);
    }

    [ClientRpc]
    public void ChangeUnpackStateClientRpc(bool value)
    {
        info.ChangeUnpackState(value);
    }

    public void ChangeNameItem(string nameID)
    {
        info.ChangeNameItem(nameID);
    }

    private void Reset()
    {
        _netDurability.Value = 100;

        _currentDurabilyItem = 100;
        _currentDurabilyBox = 100;

        info.ChangeItemState(ItemState.Arrived);

        PackItem();
    }
    private void InitArrayItems()
    {
        string path = "";
        switch (_itemType)
        {
            case ItemType.Box1x1:
                path = "Box_1x1";
                break;
            case ItemType.Box1x2:
                path = "Box_1x2";
                break;
            case ItemType.Box0x4:
                path = "Box_0x4";
                break;
            default:
                Debug.Log("Неизвестное состояние");
                break;
        }

        _items = Resources.LoadAll<ItemScriptableObject>("ItemData/" + path);
    }

    private void OnDurabilityChanged(int oldValue, int newValue)
    {
        _itemMaterial.SetFloat("_Durability", newValue);

        PlayDamageAnimation();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServer) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            return;
        }

        if (Time.time - _lastDamageTime < _damageCooldown)
            return;

        float force = collision.relativeVelocity.magnitude;

        if (force > 5f)
        {
            int damage = (int)force;
            if (info.IsBox)
            {
                _currentDurabilyBox -= damage;
                Damage(_currentDurabilyBox);
            }
            else
            {
                _currentDurabilyItem -= damage;
                Damage(_currentDurabilyItem);
            }

            _lastDamageTime = Time.time;
        }

    }

    private void Damage(int value)
    {
        _netDurability.Value = value;

        if (_netDurability.Value <= 0)
        {

            if (info.IsBox == true)
            {
                UnpacItem();
            }
            else
            {
                StartCoroutine(DestroyItemCoroutine());
            }

        }
    }

    public void PackItem()
    {
        _currentDurabilyBox = 100;
        _netDurability.Value = 100;

        _meshFilter.mesh = _cartonBox;
        _isPacked.Value = true;
    }

    public void UnpacItem()
    {
        if (!IsServer) return;

        if (info.IsBox == true)
        {
            _netDurability.Value = _currentDurabilyItem;
            int index = UnityEngine.Random.Range(0, _items.Length);
            UnpackItemClientRpc(index);

            _meshFilter.mesh = _currentMesh;
            _isPacked.Value = false;
        }
    }

    private IEnumerator DestroyItemCoroutine()
    {
        DestroyItemClientRpc();

        yield return new WaitForSeconds(0.5f);

        NetworkObject netObj = GetComponent<NetworkObject>();

        info.ChangeItemState(ItemState.Dispatched);
        OnDestroyItem?.Invoke();

        gameObject.SetActive(false);


        if (netObj != null && netObj.IsSpawned)
            netObj.Despawn(false);
    }

    [ClientRpc]
    private void DestroyItemClientRpc()
    {
        EffectUnpack();
        info.ChangeUnpackState(false);
    }

    [ClientRpc]
    private void UnpackItemClientRpc(int index)
    {
        _itemMaterial.SetFloat("_Durability", 100);

        if (info.IsUnpack == false)
        {
            _currentMesh = _items[index].mesh;
            info.nameKeyItem = _items[index].nameKeyItem;
            info.icon = _items[index].icon;

            if (_items[index].texture != null)
            {
                _currentTexture = _items[index].texture;
            }

            info.ChangeUnpackState(true);
        }

        _propertyBlock.SetTexture("_BaseTexture", _currentTexture);
        _meshRenderer.SetPropertyBlock(_propertyBlock);
        _meshFilter.mesh = _currentMesh;
        EffectUnpack();
    }

    private void OnPackedStateChanged(bool oldValue, bool newValue)
    {
        if (newValue)
        {
            info.ChangeBoxState(true);
            _meshFilter.mesh = _cartonBox;
            _propertyBlock.Clear();
            _meshRenderer.SetPropertyBlock(_propertyBlock);
            info.ChangeBoxState(true);
        }
        else
        {
            info.ChangeBoxState(false);
        }
    }

    #region Animation Unpack

    private void EffectUnpack()
    {
        if (_particalSystem != null) _particalSystem.Play();
    }

    #endregion
    #region Animation Damage
    private void PlayDamageAnimation()
    {
        if (_audioSource != null & _clipsDrop.Length > 0)
        {
            _audioSource.PlayOneShot(_clipsDrop[UnityEngine.Random.Range(0, _clipsDrop.Length)]);
        }
        StopDamageAnimation();

        _damageSequence = DOTween.Sequence();

        if (_itemMaterial != null)
        {
            _damageSequence.Append(_itemMaterial.DOColor(Color.red, "_BaseColor", damageAnimationDuration * 0.5f));
            _damageSequence.Append(_itemMaterial.DOColor(Color.black, "_BaseColor", damageAnimationDuration * 0.5f));
        }

        _damageSequence.Insert(0, transform.DOPunchScale(
            Vector3.one * scalePunch,
            damageAnimationDuration,
            1,
            0.5f
        ));

        _damageSequence.OnComplete(() =>
        {
            if (_itemMaterial != null)
            {
                _itemMaterial.color = _originalColor;
            }
            transform.localScale = _originalScale;
        });

        _damageSequence.OnKill(() =>
        {
            if (_itemMaterial != null)
            {
                _itemMaterial.color = _originalColor;
            }
            transform.localScale = _originalScale;
        });
    }

    private void StopDamageAnimation()
    {
        if (_damageSequence != null && _damageSequence.IsActive())
        {
            _damageSequence.Kill();
        }
        _damageSequence = null;
    }
    #endregion

    public override void OnNetworkDespawn()
    {
        _netDurability.OnValueChanged -= OnDurabilityChanged;
        _isPacked.OnValueChanged -= OnPackedStateChanged;

        StopDamageAnimation();

        DOTween.Kill(transform);
        if (_itemMaterial != null)
        {
            DOTween.Kill(_itemMaterial);
        }
    }

}