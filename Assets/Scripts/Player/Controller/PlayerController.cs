using Unity.Netcode;
using UnityEngine;

namespace WekenDev.Player.Controller
{

    public class PlayerController : NetworkBehaviour
    {
        [SerializeField] private Rigidbody _mainBody;

        [Header("Leg Animation")]
        [SerializeField] private Transform _feetsPos;
        [SerializeField] private Transform _leftLeg;
        [SerializeField] private Transform _rightLeg;
        private float _swingAmount = 90f;
        private float _stepSpeed = 15f;

        [Header("Movement")]
        [SerializeField] private float _speed = 5f;
        [SerializeField] private float _uprightStrength = 600f;
        [SerializeField] private Transform _cameraPivot;

        [Header("Jump")]
        [SerializeField] private float _jumpForce = 5f;
        [SerializeField] private LayerMask _groundLayer = ~0;
        //Анимация ног
        private float _swingPhase = 0f; // Фаза для синусоиды

        // Запоминаем начальное вращение ног
        private Quaternion _leftLegStartRotation = Quaternion.Euler(-80f, 0f, 0f);
        private Quaternion _rightLegStartRotation = Quaternion.Euler(-80f, 0f, 0f);

        private Vector3 _cachedForward;
        private Vector3 _cachedRight;

        //Ввод передвижения
        private Vector2 _moveInput;
        private bool _jumpInput;
        private bool _leftHand;
        private bool _rightHand;
        private NetworkVariable<bool> _netMove = new();

        public void GetInput(Vector2 moveInput, bool jump, bool leftHand, bool rightHand)
        {
            _moveInput = moveInput;
            _jumpInput = jump;
            _leftHand = leftHand;
            _rightHand = rightHand;
        }


        private void Update()
        {
            AnimationWalk();
        }

        private void AnimationWalk()
        {
            if (_netMove.Value)
            {
                float swing = Mathf.Sin(_swingPhase) * 90f;

                _leftLeg.localRotation = Quaternion.Euler(0, swing, 0) * _leftLegStartRotation;
                _rightLeg.localRotation = Quaternion.Euler(0, swing * 0.7f, 0) * _rightLegStartRotation;

                _swingPhase += 15f * Time.deltaTime;
            }
            else
            {
                _leftLeg.localRotation = Quaternion.Slerp(_leftLeg.localRotation, _leftLegStartRotation, 5f * Time.deltaTime);
                _rightLeg.localRotation = Quaternion.Slerp(_rightLeg.localRotation, _rightLegStartRotation, 5f * Time.deltaTime);
                _swingPhase = 0f;
            }
        }



        private void FixedUpdate()
        {
            if (!IsServer) return;

            HandleMovement();

            if (_moveInput.magnitude > 0.1f || _leftHand == true || _rightHand == true) StabilizeSingleBody();

        }

        private void HandleMovement()
        {
            // Движение
            if (_moveInput.magnitude > 0.1f)
            {
                if (!_netMove.Value) _netMove.Value = true;
                UpdateCameraCache();
                Vector3 moveDir = (_cachedForward * _moveInput.y + _cachedRight * _moveInput.x).normalized;
                moveDir.y = 0;

                // Горизонтальная сила - отдельно
                _mainBody.AddForce(moveDir * _speed * 100f, ForceMode.Force);
            }
            else
            {
                if (_netMove.Value) _netMove.Value = false;
            }

            // Прыжок
            if (_jumpInput && IsGrounded())
            {
                // 2. Применяем ТОЛЬКО вертикальную силу (Impulse для мгновенного толчка)
                _mainBody.AddForce(Vector3.up * _jumpForce * 100f, ForceMode.Impulse);

                Vector3 horizontalVel = new Vector3(_mainBody.linearVelocity.x, 0, _mainBody.linearVelocity.z);
                _mainBody.AddForce(horizontalVel.normalized * 1.3f, ForceMode.VelocityChange);

                _jumpInput = false;
            }
        }

        private bool IsGrounded()
        {
            Vector3 start = _feetsPos.position;
            float _groundCheckRadius = 0.1f;
            float _groundCheckHeight = 0.48f;
            Vector3 end = start + Vector3.up * _groundCheckHeight;

            return Physics.CheckCapsule(start, end, _groundCheckRadius, _groundLayer);
        }

        private void UpdateCameraCache()
        {
            Transform camTransform = _cameraPivot.transform;
            _cachedForward = camTransform.forward;
            _cachedRight = camTransform.right;

            // Обнуляем Y компоненты для горизонтального движения
            _cachedForward.y = 0;
            _cachedRight.y = 0;

            // Нормализуем только если необходимо
            if (Mathf.Abs(_cachedForward.sqrMagnitude - 1f) > 0.01f)
                _cachedForward.Normalize();

            if (Mathf.Abs(_cachedRight.sqrMagnitude - 1f) > 0.01f)
                _cachedRight.Normalize();
        }

        private void StabilizeSingleBody()
        {
            // Быстрая проверка угла отклонения
            float dot = Vector3.Dot(_mainBody.transform.up, Vector3.up);

            // Только если нужно корректировать
            Vector3 axis = Vector3.Cross(_mainBody.transform.up, Vector3.up);
            float correctionStrength = (1f - dot) * _uprightStrength * 100f;

            _mainBody.AddTorque(axis * correctionStrength);
        }
    }
}