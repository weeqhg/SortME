using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WekenDev.VoiceChat;

namespace WekenDev.Settings.Sound
{
    public class ResponseMicrophone : MonoBehaviour
    {
        [SerializeField] private TMP_Text _responseText;
        [SerializeField] private Slider _responseSlider;
        private Recorder _recoder;

        public void Init()
        {
            if (Recorder.Instance != null) _recoder = Recorder.Instance;
            else
            {
                Debug.Log($"Не найден компонентн записи звука"); 
                return;
            }

            InitializeSliders();
            LoadResponseSettings();
        }

        private void InitializeSliders()
        {
            if (_responseSlider != null)
            {
                _responseSlider.minValue = 0f;
                _responseSlider.maxValue = 100f;
                _responseSlider.onValueChanged.AddListener(SetResponse);
            }
        }

        private void LoadResponseSettings()
        {
            float savedValue = PlayerPrefs.GetFloat("ResponseMic", 50f);

            if (savedValue > 100f)
            {
                savedValue = 50f; // Сбрасываем на значение по умолчанию
                Debug.LogWarning($"Invalid saved value ({savedValue}), resetting to default");
            }

            SetResponse(savedValue);

            if (_responseSlider != null)
                _responseSlider.value = savedValue;

        }

        private void SetResponse(float value)
        {
            if (_responseText != null)
            {
                float displayValue = value;
                _responseText.text = $"{displayValue:F0}";
            }

            _recoder.SetResponseMicrophone(value);

            PlayerPrefs.SetFloat("ResponseMic", value);
            PlayerPrefs.Save();
        }

    }
}
