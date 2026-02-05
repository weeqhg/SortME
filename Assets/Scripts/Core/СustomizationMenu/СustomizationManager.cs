using UnityEngine;
using UnityEngine.InputSystem;
using WekenDev.InputSystem;
public interface ICustomizationMenu
{
    void Show();
    void Hide();
}

namespace WekenDev.СustomizationMenu
{

    public class СustomizationManager : MonoBehaviour, ICustomizationMenu
    {
        [SerializeField] private GameObject _camera;
        private InputAction _actionEscape;
        private СustomizationUI _customUI;
        private IMainMenu _mainMenu;
        private IGameManager _gameManager;
        [SerializeField] private CustomizationCamera _cameraController;
        private HSVColorWheel _hsvColor;

        public void Init(IMainMenu mainMenu, IGameManager gameManager)
        {
            _mainMenu = mainMenu;
            _camera = GetComponentInChildren<Camera>().gameObject;
            if (_camera != null) _camera.SetActive(false);

            _gameManager = gameManager;

            _customUI = GetComponent<СustomizationUI>();
            _customUI.Init();
            if (_customUI != null) _customUI.OnBackToggle += HandleBack;

            _actionEscape = InputManager.Instance.Actions.UI.Cancel;
            if (_actionEscape != null) _actionEscape.started += HandleEscapeKey;

            Hide();
            _hsvColor = GetComponentInChildren<HSVColorWheel>();
            _hsvColor.Init();

            _cameraController.Init();

            ClothMenu clothMenu = GetComponentInChildren<ClothMenu>();
            clothMenu.Init();
        }

        public void Show()
        {
            if (_gameManager != null) _gameManager.SwitchCurrentState(GameState.Customize);

            if (_camera != null) _camera.SetActive(true);

            _customUI.Show();
        }
        public void Hide()
        {
            if (_gameManager != null) _gameManager.SwitchCurrentState(GameState.MainMenu);

            if (_camera != null) _camera.SetActive(false);
            
            _customUI.Hide();
        }


        private void HandleEscapeKey(InputAction.CallbackContext context)
        {
            Hide();
        }

        private void HandleBack()
        {
            Hide();
            _mainMenu?.Show();
        }

        private void OnDestroy()
        {
            if (_actionEscape != null) _actionEscape.started -= HandleEscapeKey;
            if (_customUI != null) _customUI.OnBackToggle -= HandleBack;
        }
    }
}
