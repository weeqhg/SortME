using System;
using UnityEngine;
using UnityEngine.InputSystem;
using WekenDev.GameMenu.UI;
using WekenDev.GameMenu.Voice;
using WekenDev.InputSystem;

public interface IGameMenuController
{
    void HideMenu();
    event Action OnLeaveGame;
}


namespace WekenDev.GameMenu
{
    public class GameMenuManager : MonoBehaviour, IGameMenuController
    {
        private GameMenuUI _gameMenuUI;
        private VoiceHudGameMenu _voiceHudGameMenu;
        private IGameManager _gameManager;
        private ISettings _settings;
        private InputAction _actionPlayer;
        private InputAction _actionUI;
        public event Action OnLeaveGame;

        public void HideMenu()
        {
            _gameMenuUI.HideGeneralMenu();
        }

        public void Init(ISettings setting, IGameManager gameManager)
        {
            _voiceHudGameMenu = GetComponentInChildren<VoiceHudGameMenu>();

            _gameManager = gameManager;

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
            if (_gameManager == null || _gameManager?.GetCurrentState() != GameState.MainMenu)
            {
                _gameMenuUI.ShowGameMenu();
            }
        }

        private void HandleSettingsToggled()
        {
            _settings?.Show();
        }

        private void HandleLeaveLobby()
        {
            OnLeaveGame?.Invoke();
        }

        private void HandleShowMenu(InputAction.CallbackContext context)
        {
            AudioManager.Instance?.PlayAudioUI(TypeUiAudio.Button);
            if (_gameManager == null)
            {
                InputManager.Instance.ChangeInputType(InputType.UI);
                _gameMenuUI.ShowGameMenu();
                return;
            }
            if (_gameManager?.GetCurrentState() != GameState.MainMenu)
            {
                _gameManager?.SwitchCurrentState(GameState.Paused);
                _gameMenuUI.ShowGameMenu();
            }
        }

        private void HandleHideMenu(InputAction.CallbackContext context)
        {
            AudioManager.Instance?.PlayAudioUI(TypeUiAudio.Button);
            // 1. Проверка на null ПЕРВОЙ
            if (_gameManager == null)
            {
                InputManager.Instance.ChangeInputType(InputType.Player);
                _gameMenuUI.HideGeneralMenu();
                return; // Выходим
            }
            // 2. Проверка состояния игры
            if (_gameManager?.GetCurrentState() != GameState.MainMenu)
            {
                _gameManager?.SwitchCurrentState(GameState.Playing);
                _gameMenuUI.HideGeneralMenu();
            }
        }

        private void OnEscpaeDisable()
        {
            if (_gameManager != null) _gameManager?.SwitchCurrentState(GameState.Playing);
            else InputManager.Instance.ChangeInputType(InputType.Player);
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
