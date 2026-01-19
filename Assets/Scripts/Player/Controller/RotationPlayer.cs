using UnityEngine;

namespace WekenDev.Player.Controller
{

    public class RotationPlayer : MonoBehaviour
    {
        [SerializeField] private Transform _cameraPivot;

        [Header("Ragdoll References")]
        [SerializeField] private float _radius;
        [SerializeField] private Transform _bodyPos;
        [SerializeField] private Rigidbody _body; // Самый важный Rigidbody для вращения

        [Header("Player Rotation")]
        [SerializeField] private float _playerTurnSpeed = 5f;
        [SerializeField] private float _maxRotationAngle = 45f;
        private Vector2 _lookInput;

        private float _yaw = 0f;
        private float _pitch = 0f;

        public void GetInput(Vector2 lookInput)
        {
            _lookInput = lookInput;
        }

        private void Update()
        {
            UpdateCameraTarget();
        }
        private void FixedUpdate()
        {
            Vector3 toCamera = _cameraPivot.position - _bodyPos.position;
            toCamera.y = 0;

            if (toCamera.magnitude > 0.1f)
            {
                float targetYaw = Mathf.Atan2(toCamera.x, toCamera.z) * Mathf.Rad2Deg;
                float currentYaw = _body.rotation.eulerAngles.y;
                float angleDiff = Mathf.DeltaAngle(currentYaw, targetYaw);

                Vector3 torque = Vector3.up * (angleDiff * Mathf.Deg2Rad * _playerTurnSpeed);
                _body.AddTorque(torque, ForceMode.Force);
            }
        }


        private void UpdateCameraTarget()
        {
            Vector2 mouseDelta = _lookInput;
            
            // Обновляем углы
            _yaw += mouseDelta.x;
            _pitch -= mouseDelta.y;
            _pitch = Mathf.Clamp(_pitch, -_maxRotationAngle, _maxRotationAngle);

            // Создаем вращение из углов
            Quaternion orbitRotation = Quaternion.Euler(_pitch, _yaw, 0f);

            // Позиция камеры на орбите
            Vector3 orbitPosition = _bodyPos.position +
                                   orbitRotation * Vector3.back * _radius;

            // Устанавливаем позицию
            _cameraPivot.position = orbitPosition;

            _cameraPivot.rotation = orbitRotation;

            Vector3 lookDirection = (_cameraPivot.position - _bodyPos.position).normalized;
            _cameraPivot.rotation = Quaternion.LookRotation(lookDirection);
        }
    }
}