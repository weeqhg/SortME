using System;
using UnityEngine;
using UnityEngine.UI;

namespace WekenDev.Settings
{
    public class SettingUI : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _mainSetting;
        [SerializeField] private CanvasGroup _generalSetting;
        [SerializeField] private CanvasGroup _soundSetting;
        [SerializeField] private CanvasGroup _graphicSetting;

        [SerializeField] private Button _generalButton;
        [SerializeField] private Button _soundButton;
        [SerializeField] private Button _graphicButton;
        [SerializeField] private Button _backButton;

        public event Action OnSettingsDisableUI;

        public void Init()
        {
            _generalButton.onClick.AddListener(ShowGeneraly);
            _soundButton.onClick.AddListener(ShowSound);
            _graphicButton.onClick.AddListener(ShowGraphics);
            _backButton.onClick.AddListener(HideSetting);
        }

        public void ShowSetting()
        {
            _mainSetting.alpha = 1f;
            _mainSetting.interactable = true;
            _mainSetting.blocksRaycasts = true;
        }

        public void HideSetting()
        {
            _mainSetting.alpha = 0f;
            _mainSetting.interactable = false;
            _mainSetting.blocksRaycasts = false;
            
            OnSettingsDisableUI?.Invoke();
        }

        private void ShowGeneraly()
        {
            HideAll();

            _generalSetting.alpha = 1f;
            _generalSetting.interactable = true;
            _generalSetting.blocksRaycasts = true;
        }

        private void ShowSound()
        {
            HideAll();

            _soundSetting.alpha = 1f;
            _soundSetting.interactable = true;
            _soundSetting.blocksRaycasts = true;
        }

        private void ShowGraphics()
        {
            HideAll();

            _graphicSetting.alpha = 1f;
            _graphicSetting.interactable = true;
            _graphicSetting.blocksRaycasts = true;
        }

        private void HideAll()
        {
            _generalSetting.alpha = 0f;
            _generalSetting.interactable = false;
            _generalSetting.blocksRaycasts = false;

            _soundSetting.alpha = 0f;
            _soundSetting.interactable = false;
            _soundSetting.blocksRaycasts = false;

            _graphicSetting.alpha = 0f;
            _graphicSetting.interactable = false;
            _graphicSetting.blocksRaycasts = false;
        }
    }
}