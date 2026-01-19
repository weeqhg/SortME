using UnityEngine;

namespace WekenDev.Player.Controller
{

    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private Rigidbody _mainBody;

        [Header("Leg Animation")]
        [SerializeField] private Transform _feetsPos;
        [SerializeField] private Transform _leftLeg;
        [SerializeField] private Transform _rightLeg;
        private float _swingAmount = 180f;
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
        private float _targetSwing = 0f;
        private float _currentSwing = 0f;

        // Запоминаем начальное вращение ног
        private Quaternion _leftLegStartRotation = Quaternion.Euler(0f, 0f, 180f);
        private Quaternion _rightLegStartRotation = Quaternion.Euler(0f, 0f, 180f);

        private Vector3 _cachedForward;
        private Vector3 _cachedRight;

        //Ввод передвижения
        private Vector2 _moveInput;
        private bool _jumpInput;

        public void GetInput(Vector2 moveInput, bool jump)
        {
            _moveInput = moveInput;
            _jumpInput = jump;
        }

        private void Update()
        {
            AnimationWalk();
        }

        private void AnimationWalk()
        {
            if (_moveInput.magnitude > 0.1f)
            {
                // Синусоидальное движение для плавности
                _swingPhase += _stepSpeed * Time.deltaTime;
                _targetSwing = Mathf.Sin(_swingPhase) * _swingAmount;

                // Плавно интерполируем к целевому углу
                _currentSwing = Mathf.Lerp(_currentSwing, _targetSwing, 10f * Time.deltaTime);

                // ПРИМЕНЯЕМ вращение ОТНОСИТЕЛЬНО начального
                _leftLeg.localRotation = _leftLegStartRotation * Quaternion.Euler(_currentSwing, 0, 0);
                _rightLeg.localRotation = _rightLegStartRotation * Quaternion.Euler(-_currentSwing * 0.7f, 0, 0);
            }
            else
            {
                // Плавно возвращаем к начальному вращению
                _currentSwing = Mathf.Lerp(_currentSwing, 0f, 5f * Time.deltaTime);

                _leftLeg.localRotation = Quaternion.Slerp(
                    _leftLeg.localRotation,
                    _leftLegStartRotation,
                    5f * Time.deltaTime
                );

                _rightLeg.localRotation = Quaternion.Slerp(
                    _rightLeg.localRotation,
                    _rightLegStartRotation,
                    5f * Time.deltaTime
                );

                // Сбрасываем фазу
                _swingPhase = 0f;
            }
        }



        private void FixedUpdate()
        {
            HandleMovement();
            if (_moveInput.magnitude > 0.1f)
            {
                StabilizeSingleBody();
            }
        }

        private void HandleMovement()
        {
            // Движение
            if (_moveInput.magnitude > 0.1f)
            {
                UpdateCameraCache();
                Vector3 moveDir = (_cachedForward * _moveInput.y + _cachedRight * _moveInput.x).normalized;
                moveDir.y = 0;

                // Горизонтальная сила - отдельно
                _mainBody.AddForce(moveDir * _speed * 100f, ForceMode.Force);
            }

            // Прыжок
            if (_jumpInput && IsGrounded())
            {
                // 1. Сбрасываем вертикальную скорость перед прыжком
                Vector3 velocity = _mainBody.linearVelocity;
                velocity.y = 0;
                _mainBody.linearVelocity = velocity;

                // 2. Применяем ТОЛЬКО вертикальную силу (Impulse для мгновенного толчка)
                _mainBody.AddForce(Vector3.up * _jumpForce * 100f, ForceMode.Impulse);

                Vector3 horizontalVel = new Vector3(_mainBody.linearVelocity.x, 0, _mainBody.linearVelocity.z);
                _mainBody.AddForce(horizontalVel.normalized * 1.3f, ForceMode.VelocityChange);

                _jumpInput = false;
            }
        }

        private bool IsGrounded()
        {
            float radius = 0.2f;
            return Physics.CheckSphere(_feetsPos.position, radius, _groundLayer);
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

        private void OnDrawGizmos()
        {
            // 1. Рисуем луч
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(_feetsPos.position, 0.2f);
        }
    }
}