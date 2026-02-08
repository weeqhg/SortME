using UnityEngine;
using System.Collections;

public enum AudioDesign
{
    Mute,
    Calm,
    Intense
}
namespace WekenDev.AudioManagerGame
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioMusicController : MonoBehaviour
    {
        [Header("Music")]
        [SerializeField] private AudioClip[] _musicCalm;
        [SerializeField] private AudioClip[] _musicIntense;

        private AudioSource _musicAudio;
        private AudioDesign _currentAudiodesign;

        public void Init()
        {
            _musicAudio = GetComponent<AudioSource>();
        }

        //Music
        public void ChangeAudioDesign(AudioDesign audioDesign)
        {
            switch (audioDesign)
            {
                case AudioDesign.Mute:
                    _musicAudio.Stop();
                    break;
                case AudioDesign.Calm:
                    if (_musicCalm.Length > 0) SwitchToNextTrack(AudioDesign.Calm);
                    break;
                case AudioDesign.Intense:
                    if (_musicIntense.Length > 0) SwitchToNextTrack(AudioDesign.Intense);
                    break;
            }
        }

        private void SwitchToNextTrack(AudioDesign audioDesign)
        {
            _currentAudiodesign = audioDesign;

            StartCoroutine(FadeOutAndPlayNext());
        }

        private IEnumerator FadeOutAndPlayNext()
        {
            if (_musicAudio.clip != null)
            {
                float fadeDuration = 3f; // Увеличь длительность
                float startVolume = _musicAudio.volume;

                // Плавное затухание
                while (_musicAudio.volume > 0)
                {
                    _musicAudio.volume -= Time.deltaTime / fadeDuration;
                    yield return null;
                }

                _musicAudio.Stop();
                _musicAudio.volume = startVolume;

                yield return new WaitForSeconds(0.5f); // Пауза перед следующим
            }

            StartPlayMusic();
        }

        private void StartPlayMusic()
        {
            AudioClip clip = null;

            if (_currentAudiodesign == AudioDesign.Calm) clip = _musicCalm[Random.Range(0, _musicCalm.Length)];
            else if (_currentAudiodesign == AudioDesign.Intense) clip = _musicIntense[Random.Range(0, _musicIntense.Length)];

            if (clip != null) _musicAudio.clip = clip;
            _musicAudio.Play();

            StartCoroutine(WaitForNextTrack());
        }

        private IEnumerator WaitForNextTrack()
        {
            while (_musicAudio.isPlaying)
            {
                yield return null;
            }

            StartPlayMusic();
        }
        ///////
    }
}
