using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using WekenDev.InputSystem;
using WekenDev.Player.Controller;

namespace WekenDev.Player
{

    public class PlayerNetwork : NetworkBehaviour
    {
        [Header("Network Settings")]
        [SerializeField] private GameObject _myCamera;
        [SerializeField] private GameObject _body;
        [SerializeField] private float _sendRate = 0.05f; // 20 раз в секунду
        private ChangeColorPlayer _colorPlayer;
        private float _sendTimer = 0f;

        //Ввод игрока
        private InputAction _moveInput;
        private InputAction _lookInput;
        private InputAction _leftMouse;
        private InputAction _rightMouse;
        private InputAction _jumpInput;

        //Ссылки на управление игрока
        private ControllerHands _handsController;
        private RotationPlayer _cameraController;
        private PlayerController _playerController;
        private ChangeClothPlayer _clothPlayer;

        //Активация рук
        private bool _leftHandActive;
        private bool _rightHandActive;

        //Чуствительность мыши у каждого игрока
        private float _sensitivity = 1f;

        //Прыжок
        private bool _isJump;

        public void Init()
        {
            _handsController = GetComponent<ControllerHands>();
            _cameraController = GetComponent<RotationPlayer>();
            _playerController = GetComponent<PlayerController>();
            _clothPlayer = GetComponent<ChangeClothPlayer>();
            _colorPlayer = GetComponent<ChangeColorPlayer>();
            
            _playerController.Init();
            _colorPlayer.Init();
            _clothPlayer.Init();
            if (!IsOwner)
            {
                return;
            }

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
            if (!IsOwner) return;
            if (_moveInput == null || _lookInput == null) return;

            Vector2 moveInput = _moveInput.ReadValue<Vector2>();
            Vector2 lookInput = _lookInput.ReadValue<Vector2>();
            Vector2 mouseDelta = lookInput * _sensitivity;

            // Таймер для оптимизированной отправки
            _sendTimer += Time.deltaTime;
            if (_sendTimer >= _sendRate)
            {
                _handsController.GetInput(_leftHandActive, _rightHandActive);

                SendAllInputsServerRpc(moveInput, mouseDelta, _leftHandActive, _rightHandActive, _isJump);

                _sendTimer = 0f;
                _isJump = false;
            }
        }


        [ServerRpc]
        private void SendAllInputsServerRpc(Vector2 moveInput, Vector2 lookInput, bool leftHand, bool rightHand, bool jump)
        {
            _playerController.GetInput(moveInput, jump, leftHand, rightHand);

            _cameraController.GetInput(lookInput);

            SendHandsInputClientRpc(leftHand, rightHand);
        }

        [ClientRpc]
        private void SendHandsInputClientRpc(bool leftHand, bool rightHand)
        {
            if (!IsOwner && _handsController != null) _handsController.GetInput(leftHand, rightHand);
        }

        private void HandheldSensitivityChange(float value)
        {
            _sensitivity = value;
        }

        public override void OnNetworkDespawn()
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

}