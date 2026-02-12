using UnityEngine;
using DG.Tweening;
using Unity.Netcode;

public enum ItemType
{
    Box1x1,
    Box1x2
}


[RequireComponent(typeof(Rigidbody))]
public class DurabilityItem : NetworkBehaviour
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

    private ItemScriptableObject[] _items;
    private Mesh _cartonBox;
    private Color _originalColor = Color.black;
    private Vector3 _originalScale;
    private Material _itemMaterial;
    private MeshFilter _meshFilter;
    private ParticleSystem _particalSystem;
    private Sequence _damageSequence;
    private ItemInfo _itemInfo;
    private NetworkVariable<int> _netDurability = new NetworkVariable<int>(100);
    private bool _isUnpack = false;
    private Mesh _currentMesh;
    private Texture _currentTexture;
    private MeshRenderer _meshRenderer;
    private MaterialPropertyBlock _propertyBlock;

    void Start()
    {
        _audioSource = GetComponentInChildren<AudioSource>();
        _itemInfo = GetComponent<ItemInfo>();
        _particalSystem = GetComponentInChildren<ParticleSystem>();
        _meshFilter = GetComponent<MeshFilter>();
        _cartonBox = _meshFilter.mesh;

        _meshRenderer = GetComponent<MeshRenderer>(); // ← добавить

        _propertyBlock = new MaterialPropertyBlock();

        if (_meshRenderer != null)
        {
            _itemMaterial = _meshRenderer.material;
            _originalColor = Color.black;
        }
        _originalScale = transform.localScale;

        InitArrayItems();
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
            default:
                Debug.Log("Неизвестное состояние");
                break;
        }
        _items = Resources.LoadAll<ItemScriptableObject>("ItemData/" + path);
    }

    public override void OnNetworkSpawn()
    {
        _netDurability.OnValueChanged += OnDurabilityChanged;
    }

    private void OnDurabilityChanged(int oldValue, int newValue)
    {
        _itemMaterial.SetFloat("_Durability", newValue);

        PlayDamageAnimation();

        Debug.Log($"{gameObject.name}: осталось прочности {_netDurability.Value:F1}");
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServer) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            return;
        }

        float force = collision.relativeVelocity.magnitude;

        if (force > 5f)
        {
            int damage = (int)force * 2;
            if (_itemInfo.IsBox)
            {
                _currentDurabilyBox -= damage;
                Damage(_currentDurabilyBox);
            }
            else
            {
                _currentDurabilyItem -= damage;
                Damage(_currentDurabilyItem);
            }
        }

    }

    private void Damage(int value)
    {
        _netDurability.Value = value;

        if (_netDurability.Value <= 0)
        {
            if (_items.Length > 0)
            {
                if (_itemInfo.IsBox == true)
                {
                    UnpacItem();
                }
                else
                {
                    Debug.Log("Уничтожить предмет");
                }
            }
        }
    }

    public void UnpacItem()
    {
        if (!IsServer) return;

        if (_itemInfo.IsBox == true)
        {
            _netDurability.Value = _currentDurabilyItem;
            int index = Random.Range(0, _items.Length);
            UnpackItemClientRpc(index);
            _itemInfo.ChangeBoxState(false);
        }
    }

    [ClientRpc]
    private void UnpackItemClientRpc(int index)
    {
        _itemMaterial.SetFloat("_Durability", 100);

        if (_isUnpack == false)
        {
            _currentMesh = _items[index].mesh;
            _itemInfo.nameKeyItem = _items[index].nameKeyItem;
            _itemInfo.icon = _items[index].icon;

            if (_items[index].texture != null)
            {
                _currentTexture = _items[index].texture;
            }

            _isUnpack = true;
        }

        _propertyBlock.SetTexture("_BaseTexture", _currentTexture);
        _meshRenderer.SetPropertyBlock(_propertyBlock);
        _meshFilter.mesh = _currentMesh;
        EffectUnpack();
    }


    public void PackItem()
    {
        _currentDurabilyBox = 100;
        _netDurability.Value = _currentDurabilyBox;

        _itemMaterial.SetFloat("_Durability", _currentDurabilyBox);
        _meshFilter.mesh = _cartonBox;

        // Сброс текстуры на дефолтную коробки
        _propertyBlock.Clear();
        _meshRenderer.SetPropertyBlock(_propertyBlock);

        _itemInfo.ChangeBoxState(true);
    }

    #region Animation Unpack

    private void EffectUnpack()
    {
        if (_particalSystem != null) _particalSystem.Play();
        if (_clipsDrop.Length > 0) _audioSource.PlayOneShot(_clipsDrop[Random.Range(0, _clipsDrop.Length)]);
    }

    #endregion
    #region Animation Damage
    private void PlayDamageAnimation()
    {
        if (_audioSource != null & _clipsDrop.Length > 0)
        {
            _audioSource.PlayOneShot(_clipsDrop[Random.Range(0, _clipsDrop.Length)]);
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
        StopDamageAnimation();

        DOTween.Kill(transform);
        if (_itemMaterial != null)
        {
            DOTween.Kill(_itemMaterial);
        }
    }

}