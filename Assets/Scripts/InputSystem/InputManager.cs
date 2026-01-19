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
                    Debug.Log("Input Для Player");
                    Cursor.visible = false;
                    _actions.Player.Enable();
                    _actions.UI.Disable();
                    break;
                case InputType.UI:
                    Debug.Log("Input Для UI");
                    Cursor.visible = true;
                    _actions.Player.Disable();
                    _actions.UI.Enable();
                    break;
            }
        }

        private void OnDestroy()
        {
            _actions.Disable();
        }
    }
}
