using UnityEngine;
using UnityEngine.InputSystem;
using WekenDev.InputSystem;
using WekenDev.Player.Controller;

public class TutorPlayerController : MonoBehaviour
{
    [Header("Network Settings")]
    [SerializeField] private GameObject _myCamera;
    private TutorColorPlayer _colorPlayer;

    //Ввод игрока
    private InputAction _moveInput;
    private InputAction _lookInput;
    private InputAction _leftMouse;
    private InputAction _rightMouse;
    private InputAction _jumpInput;

    //Ссылки на управление игрока
    private TutorContHands _handsController;
    private RotationPlayer _cameraController;
    private TutorPlayer _playerController;
    private TutorClothPlayer _clothPlayer;

    //Активация рук
    private bool _leftHandActive;
    private bool _rightHandActive;

    //Чуствительность мыши у каждого игрока
    private float _sensitivity = 1f;

    //Прыжок
    private bool _isJump;

    private void Start()
    {
        InputManager.Instance.ChangeInputType(InputType.Player);

        _handsController = GetComponent<TutorContHands>();
        _cameraController = GetComponent<RotationPlayer>();
        _playerController = GetComponent<TutorPlayer>();
        _clothPlayer = GetComponent<TutorClothPlayer>();
        _colorPlayer = GetComponent<TutorColorPlayer>();

        _playerController.Init();
        _colorPlayer.Init();
        _clothPlayer.Init();

        _myCamera.SetActive(true);

        if (InputManager.Instance == null)
        {
            Debug.Log("Player not found Input System");
            return;
        }

        InputManager.Instance.OnSensitivityChanged += HandheldSensitivityChange;
        _sensitivity = InputManager.Instance.SensitivityMouse;
        _leftMouse = InputManager.Instance.Actions.Player.GrabLeft;
        _rightMouse = InputManager.Instance.Actions.Player.GrabRight;
        _moveInput = InputManager.Instance.Actions.Player.Move;
        _jumpInput = InputManager.Instance.Actions.Player.Jump;
        _lookInput = InputManager.Instance.Actions.Player.Look;

        _jumpInput.performed += ctx => _isJump = true;
        _leftMouse.started += ctx => _leftHandActive = true;
        _leftMouse.canceled += ctx => _leftHandActive = false;

        _rightMouse.started += ctx => _rightHandActive = true;
        _rightMouse.canceled += ctx => _rightHandActive = false;
    }

    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    private void Update()
    {
        if (_moveInput == null || _lookInput == null) return;

        Vector2 moveInput = _moveInput.ReadValue<Vector2>();
        Vector2 lookInput = _lookInput.ReadValue<Vector2>();
        Vector2 mouseDelta = lookInput * _sensitivity;


        _handsController.GetInput(_leftHandActive, _rightHandActive);
        _cameraController.GetInput(lookInput);
        _playerController.GetInput(moveInput, _isJump, _leftHandActive, _rightHandActive);


        _isJump = false;

    }

    private void HandheldSensitivityChange(float value)
    {
        _sensitivity = value;
    }

    private void OnDestroy()
    {
        // Очистка событий Input System
        if (_leftMouse != null)
        {
            _leftMouse.started -= ctx => _leftHandActive = true;
            _leftMouse.canceled -= ctx => _leftHandActive = false;
        }

        if (_rightMouse != null)
        {
            _rightMouse.started -= ctx => _rightHandActive = true;
            _rightMouse.canceled -= ctx => _rightHandActive = false;
        }

        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnSensitivityChanged += HandheldSensitivityChange;
        }
    }
}
