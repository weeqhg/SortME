using UnityEngine;
using DG.Tweening;

public class GateOpenClose : MonoBehaviour
{
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip[] _gateSound;
    [SerializeField] private AudioClip _truckDrive;
    [SerializeField] private float _targetHeight = -1.5f;

    public void OpenGate()
    {
        Sequence sequence = DOTween.Sequence();

        sequence.AppendInterval(2f)
                .AppendCallback(() =>
                {
                    PlayRandomSound(_gateSound);
                })
                .Append(transform.DOLocalMoveY(_targetHeight, 1.5f).SetEase(Ease.OutCubic));

    }

    public void CloseGate()
    {
        Sequence sequence = DOTween.Sequence();

        sequence.AppendCallback(() => PlayRandomSound(_gateSound))
                .Append(transform.DOLocalMoveY(0, 1.5f).SetEase(Ease.OutCubic))
                .AppendInterval(0.5f)
                .AppendCallback(() => _audioSource.PlayOneShot(_truckDrive))
                .AppendInterval(1f);
    }



    private void PlayRandomSound(AudioClip[] clips)
    {
        if (clips.Length == 0) return;
        _audioSource.PlayOneShot(clips[Random.Range(0, clips.Length)]);
    }

    internal int GetID()
    {
        throw new System.NotImplementedException();
    }
}
