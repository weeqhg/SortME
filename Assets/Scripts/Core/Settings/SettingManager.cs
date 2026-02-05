using UnityEngine;
using WekenDev.Settings.Sound;
using WekenDev.Settings.General;
using WekenDev.Settings.Graphic;
using WekenDev.InputSystem;
using UnityEngine.InputSystem;
using System;

public interface ISettings
{
    void Show();
    void Hide();
    event Action OnClosed;
}

namespace WekenDev.Settings
{
    public class SettingManager : MonoBehaviour, ISettings
    {
        public event Action OnClosed;

        [Header("Ссылки на настройки")]
        //General
        [SerializeField] private LanguageManager _language;
        [SerializeField] private LimitFPS _limitFps;
        [SerializeField] private SensitivityMouse _sensitivityMouse;

        //Graphic
        [SerializeField] private ScreenResolutionSetting _screenSetting;
        [SerializeField] private WindowModeSetting _windowSettign;

        //Sound
        [SerializeField] private ResponseMicrophone _responseMic;
        [SerializeField] private VoiceChatSelectMic _selectMic;
        [SerializeField] private VoiceChatModeMic _modeMic;
        [SerializeField] private SoundVolumeSetting _soundVolume;
        [SerializeField] private VoiceChatChekMic _voiceCheckMic;

        private InputAction _actionEscape;
        private SettingUI _settingUI;

        public void Init()
        {
            _settingUI = GetComponent<SettingUI>();

            _actionEscape = InputManager.Instance.Actions.UI.Cancel;
            if (_actionEscape != null) _actionEscape.started += HandleDisableSettingKey;

            if (_settingUI != null)
            {
                _settingUI.Init();
                _settingUI.HideSetting();
                _settingUI.OnSettingsDisableUI += HandleHideSetting;
            }

            //Основное
            if (_language != null) _language.Init();
            if (_limitFps != null) _limitFps.Init();
            if (_sensitivityMouse != null) _sensitivityMouse.Init();

            //Графика
            if (_screenSetting != null) _screenSetting.Init();
            if (_windowSettign != null) _windowSettign.Init();

            //Звук
            if (_responseMic != null) _responseMic.Init();
            if (_selectMic != null) _selectMic.Init();
            if (_modeMic != null) _modeMic.Init();
            if (_soundVolume != null) _soundVolume.Init();
            if (_voiceCheckMic != null) _voiceCheckMic.Init();

        }

        public void Hide()
        {
            _settingUI.HideSetting();
        }

        public void Show()
        {
            _settingUI.ShowSetting();
        }

        private void HandleDisableSettingKey(InputAction.CallbackContext context)
        {
            AudioManager.Instance?.PlayAudioUI(TypeUiAudio.Button);
            _settingUI.HideSetting();
        }

        private void HandleHideSetting()
        {
            OnClosed?.Invoke();
        }

        private void OnDestroy()
        {
            if (_settingUI != null) _settingUI.OnSettingsDisableUI -= HandleHideSetting;
            if (_actionEscape != null) _actionEscape.started -= HandleDisableSettingKey;
        }
    }
}
