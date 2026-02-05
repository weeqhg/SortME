using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WekenDev.InputSystem;


namespace WekenDev.Settings.General
{

    public class SensitivityMouse : MonoBehaviour
    {
        [SerializeField] private Slider _slider;
        [SerializeField] private TextMeshProUGUI _text;

        public void Init()
        {
            float savedValue = PlayerPrefs.GetFloat("SensitivityMouse", 1f);

            _slider.minValue = 0.01f;
            _slider.maxValue = 3f;
            _slider.onValueChanged.AddListener(SetSensitivityMouse);
            _slider.value = savedValue;

            SetSensitivityMouse(savedValue);
        }

        private void SetSensitivityMouse(float value)
        {
            if (_text != null)
            {
                _text.text = $"{value:F2}";
            }
            InputManager.Instance.SensitivityMouse = value;
            InputManager.Instance.OnSensitivityChanged?.Invoke(value);

            PlayerPrefs.SetFloat("SensitivityMouse", value);
            PlayerPrefs.Save();
        }

        private void OnDestroy()
        {
            if (_slider != null)
                _slider.onValueChanged.RemoveListener(SetSensitivityMouse);
        }
    }

}