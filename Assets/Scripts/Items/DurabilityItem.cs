using UnityEngine;
using DG.Tweening;
using Unity.Netcode;

public enum ItemType
{
    Box1x1,
    Box1x2
}
[RequireComponent(typeof(Rigidbody))]
public class DamageableItem : NetworkBehaviour
{
    [Header("Звуки")]
    [SerializeField] private AudioClip[] _clipsDrop;
    private AudioSource _audioSource;

    [Header("Анимация удара")]
    [SerializeField] private float damageAnimationDuration = 0.3f;
    [SerializeField] private float scalePunch = 0.1f;

    [Header("Предметы")]
    [SerializeField] private ItemType _itemType;

    private ItemScriptableObject[] _items;
    private Mesh _cartonBox;
    private Color _originalColor = Color.white;
    private Vector3 _originalScale;
    private Material _itemMaterial;
    private MeshFilter _meshFilter;
    private ParticleSystem _particalSystem;
    private Sequence _damageSequence;
    private ItemInfo _itemInfo;
    private NetworkVariable<float> _netDurability = new NetworkVariable<float>(100f);
    private bool _isBox = true;

    void Start()
    {
        _audioSource = GetComponentInChildren<AudioSource>();
        _itemInfo = GetComponent<ItemInfo>();
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        _particalSystem = GetComponentInChildren<ParticleSystem>();
        _meshFilter = GetComponent<MeshFilter>();
        _cartonBox = _meshFilter.mesh;

        if (renderer != null)
        {
            _itemMaterial = renderer.material;
            _originalColor = Color.white;
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

    private void OnDurabilityChanged(float oldValue, float newValue)
    {
        _itemMaterial.SetFloat("_Durability", _netDurability.Value / 100f);

        PlayDamageAnimation();

        Debug.Log($"{gameObject.name}: осталось прочности {_netDurability.Value:F1}");
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            return;
        }

        float force = collision.relativeVelocity.magnitude;

        if (force > 5f)
        {
            float damage = force * 2f;
            _netDurability.Value -= damage;
        }

        if (_netDurability.Value <= 0)
        {
            _netDurability.Value = 100f;
            if (_items.Length > 0)
            {
                if (_isBox == true)
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
        if (_isBox == true)
        {
            int index = Random.Range(0, _items.Length);
            UnpackItemClientRpc(index);
            _isBox = false;
        }
    }

    [ClientRpc]
    private void UnpackItemClientRpc(int index)
    {
        _itemMaterial.SetFloat("_Durability", 1f);
        _meshFilter.mesh = _items[index].mesh;
        _itemInfo.nameKeyItem = _items[index].nameKeyItem;
        _itemInfo.icon = _items[index].icon;
        EffectUnpack();
    }

    [ClientRpc]
    private void PackItemClientRpc()
    {
        _itemMaterial.SetFloat("_Durability", 1f);
        _meshFilter.mesh = _cartonBox;
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
        // Останавливаем предыдущую анимацию
        StopDamageAnimation();

        // Создаем новую последовательность
        _damageSequence = DOTween.Sequence();

        // 1. Анимация цвета
        if (_itemMaterial != null)
        {
            _damageSequence.Append(_itemMaterial.DOColor(Color.red, "_BaseColor", damageAnimationDuration * 0.5f));
            _damageSequence.Append(_itemMaterial.DOColor(Color.white, "_BaseColor", damageAnimationDuration * 0.5f));
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