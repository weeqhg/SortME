using UnityEngine;
using WekenDev.MainMenu.UI;
using WekenDev.Game;
using UnityEngine.InputSystem;
using WekenDev.InputSystem;

public interface IMainMenu
{
    void Show();
    void Hide();
    bool IsActive { set; }
}

namespace WekenDev.MainMenu
{
    public class MainMenuManager : MonoBehaviour, IMainMenu
    {
        [Header("Reference")]
        private ISettings _settings;
        private Camera _camera;
        private GameManager _gameManager;
        private InputAction _actionEscape;
        private MainMenuUI _menuUI;
        private bool _isActive = true;

        public bool IsActive
        {
            set
            {
                _isActive = value;
                Debug.Log(_isActive);
            }
        }

        public void Init(ISettings settings, GameManager gameManager)
        {
            _settings = settings;
            if (_settings != null) _settings.OnClosed += Show;


            _camera = GetComponentInChildren<Camera>();
            if (_camera == null) Debug.Log($"Компонент {_camera.GetType()} не найден");
            if (_camera != null) _camera.enabled = true;


            _gameManager = gameManager;
            if (_gameManager == null) Debug.Log($"Компонент {_gameManager.GetType()} не найден");

            _actionEscape = InputManager.Instance.Actions.UI.Cancel;
            if (_actionEscape != null) _actionEscape.started += HandleEscapeKey;

            _menuUI = GetComponent<MainMenuUI>();
            if (_menuUI != null)
            {
                _menuUI.Init();
                _menuUI.OnSettingsActiveUI += HandleShowSetting;
            }
        }

        public void Show()
        {
            if (!_isActive) return; //Не очень хорошее решение в будущем нужно изменить

            if (_camera != null && !_camera.enabled) _camera.enabled = true;
            if (!Cursor.visible) Cursor.visible = true;
            if (!_gameManager.IsHaveLocalPlayer) _menuUI.ShowMainMenu();
        }

        public void Hide()
        {
            if (_camera != null) _camera.enabled = false;
            Cursor.visible = false;
        }

        private void HandleEscapeKey(InputAction.CallbackContext context)
        {
            if (_isActive) Show();
        }

        private void HandleShowSetting()
        {
            _settings?.Show();
        }

        private void OnDestroy()
        {
            if (_settings != null) _settings.OnClosed -= Show;
            if (_menuUI != null) _menuUI.OnSettingsActiveUI -= HandleShowSetting;
            if (_actionEscape != null) _actionEscape.started -= HandleEscapeKey;
        }
    }
}
