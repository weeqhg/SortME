using TMPro;
using UnityEngine;
using WekenDev.VoiceChat;

namespace WekenDev.Settings.Sound
{
    public class VoiceChatModeMic : MonoBehaviour
    {
        private TMP_Dropdown _dropdown;
        private Recorder _recoder;

        public void Init()
        {
            if (Recorder.Instance != null) _recoder = Recorder.Instance;
            else
            {
                Debug.Log($"Не найден компонентн записи звука");
                return;
            }

            _dropdown = GetComponentInChildren<TMP_Dropdown>();
            if (_dropdown != null)
            {
                LoadSave();
                _dropdown.onValueChanged.AddListener(OnMicrophoneModeChanged);
            }
        }

        //0-VoiceActivaion
        //1-Push-to-Talk

        private void OnMicrophoneModeChanged(int value)
        {
            PlayerPrefs.SetInt("MicrophoneMode", value);


            if (value == 0)
            {
                _recoder.SetMode(true);
            }
            else
            {
                _recoder.SetMode(false);
            }

        }

        private void LoadSave()
        {
            int savedValue = PlayerPrefs.GetInt("MicrophoneMode", 0);
            _dropdown.value = savedValue;
            OnMicrophoneModeChanged(savedValue);
        }

    }
}
