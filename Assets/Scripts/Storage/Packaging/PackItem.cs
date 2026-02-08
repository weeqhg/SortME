using UnityEngine;
using DG.Tweening;

public class PackItem : MonoBehaviour
{
    [SerializeField] private Transform _press;
    private float _targetPoint = -1.5f;
    [SerializeField] private AudioClip[] _audioClips;
    private AudioSource _audioSource;
    private ParticleSystem _partical;
    private void Start()
    {
        _audioSource = GetComponentInChildren<AudioSource>();
        _partical = GetComponentInChildren<ParticleSystem>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Item"))
        {
            DamageableItem damageableItem = other.GetComponent<DamageableItem>();
            if (damageableItem != null)
            {
                //if (!damageableItem.IsBox())
                {
                    PressAnimation();
                    damageableItem.PackItem();
                }
            }
        }
    }

    private void PressAnimation()
    {
        Sequence pressSequence = DOTween.Sequence();

        if (_audioSource != null) _audioSource.PlayOneShot(_audioClips[Random.Range(0, _audioClips.Length)]);
        // Опускание
        pressSequence.Append(_press.DOLocalMoveY(_targetPoint, 0.5f).SetEase(Ease.InOutCubic))
                 .AppendInterval(0.1f)
                 .AppendCallback(() =>
                 {
                     if (_partical != null) _partical.Play();
                 })
                     // Возврат
                     .Append(_press.DOLocalMoveY(0f, 0.5f).SetEase(Ease.OutBack));
    }

    private void OnDestroy()
    {
        DOTween.Kill(_press);
        DOTween.Kill(transform); // Если есть другие анимации
    }
}
