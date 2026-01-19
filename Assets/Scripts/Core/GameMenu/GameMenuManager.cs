using System;
using UnityEngine;
using UnityEngine.InputSystem;
using WekenDev.GameMenu.UI;
using WekenDev.GameMenu.Voice;
using WekenDev.InputSystem;
using WekenDev.Game;

public interface IGameMenuController
{
    void HideMenu();
    void ShowMenu();
    public event Action OnLeaveGame;
}

namespace WekenDev.GameMenu
{
    public class GameMenuManager : MonoBehaviour, IGameMenuController
    {
        private GameMenuUI _gameMenuUI;
        private VoiceHudGameMenu _voiceHudGameMenu;
        private GameManager _gameManager;
        private ISettings _settings;
        private InputAction _actionPlayer;
        private InputAction _actionUI;
        public event Action OnLeaveGame;

        public void HideMenu()
        {
            _gameMenuUI.HideGeneralMenu();
        }

        public void ShowMenu()
        {
            if (_gameManager != null && !_gameManager.IsHaveLocalPlayer) return;

            InputManager.Instance.ChangeInputType(InputType.UI);
            _gameMenuUI.ShowGeneralMenu();
        }

        public void Init(ISettings setting, GameManager gameManager)
        {
            _voiceHudGameMenu = GetComponentInChildren<VoiceHudGameMenu>();

            _settings = setting;
            if (_settings != null) _settings.OnClosed += HandleBackToggled;

            if (InputManager.Instance == null) Debug.Log($"Не найден {InputManager.Instance.GetType()}");
            _actionPlayer = InputManager.Instance.Actions.Player.Cancel;
            _actionUI = InputManager.Instance.Actions.UI.Cancel;
            if (_actionPlayer != null) _actionPlayer.started += HandleShowMenu;
            if (_actionUI != null) _actionUI.started += HandleHideMenu;

            _gameMenuUI = GetComponent<GameMenuUI>();
            _gameMenuUI.Init();
            _gameMenuUI.OnSettingActiveUI += HandleSettingsToggled;
            _gameMenuUI.OnLeaveLobbyUI += HandleLeaveLobby;
            _gameMenuUI.OnReturnGame += OnEscpaeDisable;

            _gameManager = gameManager;
            if (_gameManager != null)
            {
                _gameManager.OnNewAudioSourcePlayer += HandleAudioSource;
                _gameManager.OnDisconnectPlayer += HandleDisconnectPlayer;
            }

        }

        private void HandleAudioSource(ulong clientId, AudioSource audioSoursec)
        {
            _voiceHudGameMenu.Register(clientId, audioSoursec);
        }
        private void HandleDisconnectPlayer(ulong clientId)
        {
            _voiceHudGameMenu.Unregister(clientId);
        }


        private void HandleBackToggled()
        {
            //Есть игрок или нет
            //Если есть то показываем меню, если нет то не показываем
            if (_gameManager != null && _gameManager.IsHaveLocalPlayer) _gameMenuUI.ShowGeneralMenu();
        }

        private void HandleSettingsToggled()
        {
            Debug.Log("Показать настройки");
            _settings?.Show();
        }

        private void HandleLeaveLobby()
        {
            OnLeaveGame?.Invoke();
        }

        private void HandleShowMenu(InputAction.CallbackContext context)
        {
            if (_gameManager != null && !_gameManager.IsHaveLocalPlayer) return;

            // Показываем меню
            InputManager.Instance.ChangeInputType(InputType.UI);
            _gameMenuUI.ShowGeneralMenu();

        }

        private void HandleHideMenu(InputAction.CallbackContext context)
        {
            if (_gameManager != null && !_gameManager.IsHaveLocalPlayer) return;

            InputManager.Instance.ChangeInputType(InputType.Player);
            _gameMenuUI.HideGeneralMenu();
        }

        private void OnEscpaeDisable()
        {
            InputManager.Instance.ChangeInputType(InputType.Player);
        }

        private void OnDestroy()
        {
            _gameMenuUI.OnSettingActiveUI -= HandleSettingsToggled;
            _gameMenuUI.OnLeaveLobbyUI -= HandleLeaveLobby;
            _gameMenuUI.OnReturnGame -= OnEscpaeDisable;

            if (_settings != null) _settings.OnClosed -= HandleBackToggled;

            if (_actionPlayer != null) _actionPlayer.started -= HandleShowMenu;
            if (_actionUI != null) _actionUI.started -= HandleHideMenu;

            if (_gameManager != null)
            {
                _gameManager.OnNewAudioSourcePlayer -= HandleAudioSource;
                _gameManager.OnDisconnectPlayer -= HandleDisconnectPlayer;
            }

        }
    }
}
