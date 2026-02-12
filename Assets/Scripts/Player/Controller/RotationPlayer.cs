using UnityEngine;

namespace WekenDev.Player.Controller
{

    public class RotationPlayer : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform _cameraPivot;
        [SerializeField] private Rigidbody _body;
        private float _heightOffset = 0.5f;
        private float _radius = 1f;
        private float _playerTurnSpeed = 100f;
        private float _maxRotationAngle = 45f;
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
            Vector3 toCamera = _cameraPivot.position - _body.transform.position;
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

            Vector3 orbitCenter = _body.transform.position + Vector3.up * _heightOffset;
            Vector3 orbitPosition = orbitCenter + orbitRotation * Vector3.back * _radius;

            // Устанавливаем позицию
            _cameraPivot.position = orbitPosition;

            _cameraPivot.rotation = orbitRotation;

            Vector3 lookDirection = (_cameraPivot.position - _body.transform.position).normalized;
            _cameraPivot.rotation = Quaternion.LookRotation(lookDirection);
        }

        private void OnDrawGizmos()
        {
            if (_body == null) return;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(_body.transform.position, _radius);

            // Центр орбиты
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(_body.transform.position, 0.1f);

            // Линия к камере
            if (_cameraPivot != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(_body.transform.position, _cameraPivot.position);
            }
        }
    }


}