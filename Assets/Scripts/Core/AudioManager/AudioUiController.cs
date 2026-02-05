using UnityEngine;

public enum TypeUiAudio
{
    Mute,
    Button,
    Slider
}

namespace WekenDev.AudioManagerGame
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioUiController : MonoBehaviour
    {
        [Header("UI Sound")]
        [SerializeField] private AudioClip[] _buttons;
        [SerializeField] private AudioClip[] _sliders;
        private AudioSource _uiAudio;
        public void Init()
        {
            _uiAudio = GetComponent<AudioSource>();
        }

        public void PlayAudioUI(TypeUiAudio type)
        {
            switch (type)
            {
                case TypeUiAudio.Mute:
                    _uiAudio.Stop();
                    _uiAudio.clip = null;
                    _uiAudio.loop = false;
                    break;
                case TypeUiAudio.Button:
                    if (_buttons != null)
                    {
                        AudioClip clip = _buttons[Random.Range(0, _buttons.Length)];
                        _uiAudio.PlayOneShot(clip);
                    }
                    break;
                case TypeUiAudio.Slider:
                    if (_sliders != null)
                    {
                        AudioClip clip = _sliders[Random.Range(0, _sliders.Length)];
                        _uiAudio.clip = clip;
                        _uiAudio.loop = true;
                        _uiAudio.Play();
                    }
                    break;
            }
        }
    }
}
