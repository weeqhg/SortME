using UnityEngine;
using WekenDev.MainMenu.UI;
using UnityEngine.InputSystem;
using WekenDev.InputSystem;
public interface IMainMenu
{
    void Show();
    void Hide();
}

namespace WekenDev.MainMenu
{
    public class MainMenuManager : MonoBehaviour, IMainMenu
    {
        private ISettings _settings;
        private IGameManager _gameManager;
        private ICustomizationMenu _customMenu;
        private GameObject _camera;
        private InputAction _actionEscape;
        private MainMenuUI _menuUI;
        private AuthorUI _authorUI;

        public void Init(ISettings settings, ICustomizationMenu custom, IGameManager gameManager)
        {
            _settings = settings;
            if (_settings != null) _settings.OnClosed += Show;
            _customMenu = custom;

            _gameManager = gameManager;

            _camera = GetComponentInChildren<Camera>().gameObject;
            if (_camera == null) Debug.Log($"Компонент {_camera.GetType()} не найден");
            if (_camera != null) _camera.SetActive(true);

            _actionEscape = InputManager.Instance.Actions.UI.Cancel;
            if (_actionEscape != null) _actionEscape.started += HandleEscapeKey;
            _authorUI = GetComponentInChildren<AuthorUI>();
            
            if (_authorUI != null) _authorUI.Init(this);

            _menuUI = GetComponent<MainMenuUI>();
            if (_menuUI != null)
            {
                _menuUI.Init();
                _menuUI.OnSettingsActiveUI += HandleShowSetting;
                _menuUI.OnСustomizationMenu += HandleShowCustomize;
            }
        }

        public void Show()
        {
            if (_gameManager == null || _gameManager?.GetCurrentState() == GameState.MainMenu)
            {
                if (_camera != null && !_camera.activeSelf) _camera.SetActive(true);

                _menuUI.ShowMainMenu();
            }
        }

        public void Hide()
        {
            if (_camera != null) _camera.SetActive(false);
        }

        private void HandleEscapeKey(InputAction.CallbackContext context)
        {
            if (_gameManager == null || _gameManager?.GetCurrentState() == GameState.MainMenu)
            {
                AudioManager.Instance?.PlayAudioUI(TypeUiAudio.Button);
                Show();
            }

        }

        private void HandleShowSetting()
        {
            _settings?.Show();
        }
        private void HandleShowCustomize()
        {
            if (_customMenu == null) return;
            Hide();
            _customMenu?.Show();
        }

        private void OnDestroy()
        {
            if (_settings != null) _settings.OnClosed -= Show;
            if (_menuUI != null) _menuUI.OnSettingsActiveUI -= HandleShowSetting;
            if (_menuUI != null) _menuUI.OnСustomizationMenu -= HandleShowCustomize;
            if (_actionEscape != null) _actionEscape.started -= HandleEscapeKey;
        }
    }
}
