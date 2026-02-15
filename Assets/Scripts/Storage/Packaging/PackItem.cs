using UnityEngine;
using DG.Tweening;
using Unity.Netcode;

public class PackItem : NetworkBehaviour
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
        if (!IsServer) return;

        if (other.CompareTag("Item"))
        {
            ItemInfo itemInfo = other.GetComponent<ItemInfo>();
            if (itemInfo != null)
            {
                if (!itemInfo.IsBox)
                {
                    PressAnimationClientRpc();
                    ItemManager itemManager = other.GetComponent<ItemManager>();
                    itemManager.PackItem();
                }
            }
        }
    }

    [ClientRpc]
    private void PressAnimationClientRpc()
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

    public override void OnNetworkDespawn()
    {
        DOTween.Kill(_press);
        DOTween.Kill(transform);
    }
}
