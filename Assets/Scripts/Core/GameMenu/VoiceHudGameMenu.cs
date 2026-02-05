using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace WekenDev.GameMenu.Voice
{
    public class VoiceHudGameMenu : MonoBehaviour
    {
        [SerializeField] private Transform _parentVoiceSettingUI;
        [SerializeField] private GameObject _voicePlayerUI;
        [SerializeField] private Dictionary<ulong, AudioSource> _speakers = new();
        private Dictionary<ulong, GameObject> _uiElements = new();

        public void Register(ulong clientId, AudioSource audioSources)
        {
            if (_uiElements.ContainsKey(clientId))
            {
                Unregister(clientId);
            }

            _speakers[clientId] = audioSources;
            CreateUI(clientId);
        }

        private void CreateUI(ulong clientId)
        {
            // Создаем UI элемент
            GameObject uiObject = Instantiate(_voicePlayerUI, _parentVoiceSettingUI);
            uiObject.name = $"VoiceUI_Player_{clientId}"; // Даем понятное имя

            TextMeshProUGUI nameText = uiObject.GetComponentInChildren<TextMeshProUGUI>();
            if (nameText != null) nameText.text = "Player: " + clientId;

            Slider slider = uiObject.GetComponentInChildren<Slider>();
            if (slider != null)
            {
                // Добавляем обработчик
                slider.onValueChanged.AddListener((value) => OnVolumeChanged(clientId, value));
            }

            // Сохраняем ссылку на UI элемент
            _uiElements[clientId] = uiObject;

        }


        public void Unregister(ulong clientId)
        {
            // Удаляем UI элемент если он существует
            if (_uiElements.TryGetValue(clientId, out GameObject uiElement))
            {
                Slider slider = uiElement.GetComponentInChildren<Slider>();
                if (slider != null) slider.onValueChanged.RemoveAllListeners();

                Destroy(uiElement);
                _uiElements.Remove(clientId);
            }

            // Удаляем источник звука из словаря
            _speakers.Remove(clientId);
            Debug.Log("очистка словаря");
        }
        private void OnVolumeChanged(ulong clientId, float volume)
        {
            // Применяем к Speaker
            if (_speakers.TryGetValue(clientId, out AudioSource speaker))
            {
                if (speaker != null) speaker.volume = volume;
            }
        }

        private void OnDestroy()
        {
            _speakers.Clear();
        }
    }

}