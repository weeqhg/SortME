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
        [SerializeField] private float _fadeDuration = 1f; // длительность кроссфейда
        [SerializeField] private float _defaultVolume = 1f;

        [SerializeField] private AudioSource[] _musicSources = new AudioSource[2];
        private int _activeSourceIndex;
        private AudioDesign _currentAudiodesign;
        private Coroutine _crossfadeCoroutine;

        public void Init()
        {
            // настроим исходный source
            _musicSources[0].playOnAwake = false;
            _musicSources[0].loop = false;
            _musicSources[0].volume = _defaultVolume;

            // создаём второй источник для кроссфейда
            _musicSources[1].playOnAwake = false;
            _musicSources[1].loop = false;
            _musicSources[1].volume = 0f;

            _activeSourceIndex = 0;
        }

        //Music
        public void ChangeAudioDesign(AudioDesign audioDesign)
        {
            switch (audioDesign)
            {
                case AudioDesign.Mute:
                    StopAllMusic();
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

            // если уже идёт кроссфейд — прерываем и запускаем новый
            if (_crossfadeCoroutine != null)
            {
                StopCoroutine(_crossfadeCoroutine);
                _crossfadeCoroutine = null;
            }

            AudioSource active = _musicSources[_activeSourceIndex];

            // если сейчас ничего не играет — просто проигрываем на активном источнике без паузы
            if (!active.isPlaying)
            {
                PlayOnActiveSourceImmediate();
            }
            else
            {
                _crossfadeCoroutine = StartCoroutine(CrossfadeToNext(_fadeDuration));
            }
        }

        private void PlayOnActiveSourceImmediate()
        {
            AudioClip clip = GetClipForCurrentDesign();
            if (clip == null) return;

            AudioSource active = _musicSources[_activeSourceIndex];
            active.clip = clip;
            active.volume = _defaultVolume;
            active.Play();

            StartCoroutine(WaitForNextTrack());
        }

        private IEnumerator CrossfadeToNext(float fadeDuration)
        {
            AudioSource active = _musicSources[_activeSourceIndex];
            AudioSource next = _musicSources[1 - _activeSourceIndex];

            AudioClip clip = GetClipForCurrentDesign();
            if (clip == null) yield break;

            next.clip = clip;
            next.volume = 0f;
            next.Play();

            float t = 0f;
            float fromVol = _defaultVolume;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                float frac = Mathf.Clamp01(t / fadeDuration);
                next.volume = Mathf.Lerp(0f, fromVol, frac);
                active.volume = Mathf.Lerp(fromVol, 0f, frac);
                yield return null;
            }

            active.Stop();
            active.volume = fromVol;

            _activeSourceIndex = 1 - _activeSourceIndex;
            _crossfadeCoroutine = null;

            StartCoroutine(WaitForNextTrack());
        }

        private AudioClip GetClipForCurrentDesign()
        {
            if (_currentAudiodesign == AudioDesign.Calm)
            {
                return _musicCalm.Length > 0 ? _musicCalm[Random.Range(0, _musicCalm.Length)] : null;
            }
            else if (_currentAudiodesign == AudioDesign.Intense)
            {
                return _musicIntense.Length > 0 ? _musicIntense[Random.Range(0, _musicIntense.Length)] : null;
            }

            return null;
        }


        private IEnumerator WaitForNextTrack()
        {
            AudioSource active = _musicSources[_activeSourceIndex];
            while (active.isPlaying)
            {
                yield return null;
            }

            // когда трек закончился — проигрываем следующий тот же дизайн через кроссфейд
            // если хотите без кроссфейда — вызывайте PlayOnActiveSourceImmediate()
            if (_currentAudiodesign != AudioDesign.Mute)
            {
                _crossfadeCoroutine = StartCoroutine(CrossfadeToNext(_fadeDuration));
            }
        }

        private void StopAllMusic()
        {
            foreach (var s in _musicSources)
            {
                if (s != null)
                {
                    s.Stop();
                    s.clip = null;
                    s.volume = _defaultVolume;
                }
            }

            if (_crossfadeCoroutine != null)
            {
                StopCoroutine(_crossfadeCoroutine);
                _crossfadeCoroutine = null;
            }
        }
        ///////
    }
}
