using System.Linq;
using TMPro;
using UnityEngine;
using WekenDev.VoiceChat;

namespace WekenDev.Settings.Sound
{
    public class VoiceChatSelectMic : MonoBehaviour
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
                InitializeMicrophoneDropdown();
                _dropdown.onValueChanged.AddListener(OnMicrophoneSelected);
            }
        }

        private void InitializeMicrophoneDropdown()
        {
            // Очищаем текущие опции
            _dropdown.ClearOptions();

            // Получаем список доступных микрофонов
            string[] microphones = Microphone.devices;

            if (microphones.Length == 0)
            {
                _dropdown.interactable = false;
                _dropdown.options.Add(new TMP_Dropdown.OptionData("No microphones found"));
                return;
            }

            // Добавляем микрофоны в dropdown
            _dropdown.AddOptions(microphones.ToList());

            // Выбираем микрофон по умолчанию
            string savedMic = PlayerPrefs.GetString("SelectedMicrophone", "");
            int savedIndex = System.Array.IndexOf(microphones, savedMic);

            if (savedIndex >= 0)
            {
                _dropdown.value = savedIndex;
            }
            else if (Microphone.devices.Length > 0)
            {
                // Используем первый микрофон по умолчанию
                _dropdown.value = 0;
                OnMicrophoneSelected(0);
            }

            _dropdown.RefreshShownValue();
        }

        // Обработка выбора микрофона
        private void OnMicrophoneSelected(int index)
        {
            if (index < 0 || index >= Microphone.devices.Length) return;

            string selectedMic = Microphone.devices[index];

            // Сохраняем выбор
            PlayerPrefs.SetString("SelectedMicrophone", selectedMic);
            PlayerPrefs.Save();

            // Можно сразу применить настройку к системе голосового чата
            ApplyMicrophoneToVoiceSystem(selectedMic);
        }

        private void ApplyMicrophoneToVoiceSystem(string deviceName)
        {

            _recoder.SetMicrophone(deviceName);

            Debug.Log($"Applying microphone to voice system: {deviceName}");
        }

        private void OnDestroy()
        {
            if (_dropdown != null) _dropdown.onValueChanged.RemoveListener(OnMicrophoneSelected);
        }
    }
}
