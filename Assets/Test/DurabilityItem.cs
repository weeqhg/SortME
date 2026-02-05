using UnityEngine;
using DG.Tweening; // Добавляем DOTween

[RequireComponent(typeof(Rigidbody))]
public class DamageableItem : MonoBehaviour
{
    [Header("Звуки")]
    [SerializeField] private AudioClip[] _clipsDrop;
    private AudioSource _audioSource;
    [Header("Прочность")]
    [SerializeField] private float maxDurability = 100f;
    private float _durability = 100f;
    private bool broken = false;

    [Header("Анимация удара")]
    [SerializeField] private float damageAnimationDuration = 0.3f;
    [SerializeField] private float scalePunch = 0.1f;

    private Color _originalColor = Color.white;
    private Vector3 _originalScale;
    private Material _itemMaterial;
    private Sequence _damageSequence;

    void Start()
    {
        _audioSource = GetComponentInChildren<AudioSource>();
        Debug.Log(_audioSource);
        // Получаем материал для изменения цвета
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            _itemMaterial = renderer.material;
            _originalColor = Color.white;
        }
        _originalScale = transform.localScale;
    }

    private void OnCollisionEnter(Collision collision)
    {
        //if (broken) return;

        // Игнорируем игрока
        if (collision.gameObject.CompareTag("Player"))
        {
            return;
        }

        float force = collision.relativeVelocity.magnitude;

        if (force > 5f)
        {
            float damage = force * 2f;
            _durability -= damage;
            _itemMaterial.SetFloat("_Durability", _durability / 100f);

            Debug.Log($"{gameObject.name}: Удар силой {force:F1}, урон {damage:F1}, осталось {_durability:F1}");

            // Запускаем анимацию получения урона
            PlayDamageAnimation();

            if (_durability <= 0)
            {
                //broken = true;
                Debug.Log($"{gameObject.name}: СЛОМАЛСЯ!");

                //GetComponent<Collider>().enabled = false;
            }
        }
    }

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

    private void OnDestroy()
    {
        StopDamageAnimation();

        DOTween.Kill(transform);
        if (_itemMaterial != null)
        {
            DOTween.Kill(_itemMaterial);
        }
    }
}