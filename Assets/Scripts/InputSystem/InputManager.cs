using System;
using UnityEngine;

namespace WekenDev.InputSystem
{
    public enum InputType
    {
        Player, UI
    }
    public class InputManager : MonoBehaviour
    {
        private InputSystem_Actions _actions;
        public InputSystem_Actions Actions => _actions;

        public static InputManager Instance;

        public Action<float> OnSensitivityChanged;
        public float SensitivityMouse = 1f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            _actions = new InputSystem_Actions();
            _actions.Enable();
        }

        public void ChangeInputType(InputType inputType)
        {
            switch (inputType)
            {
                case InputType.Player:

                    Cursor.visible = false;
                    Cursor.lockState = CursorLockMode.Locked;
                    _actions.Player.Enable();
                    _actions.UI.Disable();
                    Debug.Log("PlayerInput");
                    break;
                case InputType.UI:

                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.None;
                    _actions.Player.Disable();
                    _actions.UI.Enable();
                    Debug.Log("UIInput");
                    break;
            }
        }

        private void OnDestroy()
        {
            if (_actions != null) _actions.Disable();
        }
    }
}
