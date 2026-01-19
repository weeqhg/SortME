using UnityEngine;

namespace WekenDev.Player.Controller
{

    public class TestControllerHands : MonoBehaviour
    {
        [Header("Ссылки")]
        [SerializeField] private Rigidbody _playerBody;
        [SerializeField] private Rigidbody _leftHand;
        [SerializeField] private Rigidbody _rightHand;
        [SerializeField] private Transform _cameraPivot;
        [SerializeField] private Transform _armsCenterPoint;
        [SerializeField] private Transform _leftEnd;
        [SerializeField] private Transform _rightEnd;

        //Подъем рук
        [SerializeField] private float _handForce = 400f; //Сила подъема рук
        private float _dragForce = 5f;

        //Стабилизация при ходьбе
        [SerializeField] private float downForce = 200f;

        //Ввод игрока
        private bool _leftHandActive;
        private bool _rightHandActive;


        public void GetInput(bool left, bool right)
        {
            _leftHandActive = left;
            _rightHandActive = right;
        }

        private void FixedUpdate()
        {
            if (_leftHandActive)
            {
                MoveHandToTarget(_leftHand);
            }
            else
            {
                StabilizeHand(_leftHand);
            }

            if (_rightHandActive)
            {
                MoveHandToTarget(_rightHand);
            }
            else
            {
                StabilizeHand(_rightHand);
            }

        }

        private void MoveHandToTarget(Rigidbody hand)
        {
            float sideOffset = 0.05f; //Внимание
            bool isLeftHand = (hand == _leftHand);

            Vector3 sideDirection = isLeftHand ? -_cameraPivot.right : _cameraPivot.right;
            Vector3 handEnd = isLeftHand ? _leftEnd.position : _rightEnd.position;
            Vector3 offsetTarget = _cameraPivot.position + sideDirection * sideOffset;

            Vector3 tipToHand = handEnd - hand.position;
            Vector3 targetHandPosition = offsetTarget + tipToHand;

            Vector3 toTarget = targetHandPosition - hand.position;
            float distance = toTarget.magnitude;

            if (distance > 0.01f)
            {
                float forceMultiplier = Mathf.Clamp(distance * _handForce, 0f, _handForce);
                hand.AddForce(toTarget.normalized * forceMultiplier);

                if (_playerBody != null)
                {
                    Vector3 forceDirection = -toTarget.normalized;
                    _playerBody.AddForce(forceDirection * forceMultiplier * 1f);
                }
            }
            hand.linearDamping = _dragForce;
        }

        private void StabilizeHand(Rigidbody hand)
        {
            // 1. Притяжение к позиции покоя
            bool isLeftHand = (hand == _leftHand);
            Vector3 handEnd = isLeftHand ? _leftEnd.position : _rightEnd.position;

            Vector3 handForce = Vector3.down * downForce * Time.fixedDeltaTime * 50f;

            hand.AddForceAtPosition(handForce, handEnd, ForceMode.Force);

            if (_playerBody != null)
            {
                Vector3 bodyForce = -handForce;
                _playerBody.AddForceAtPosition(bodyForce, handEnd, ForceMode.Force);
            }

            hand.linearDamping = _dragForce * 2;
        }

        private void OnDrawGizmos()
        {
            if (_playerBody == null) return;

            DrawSimpleForce(_leftHand, _leftHandActive, Color.red);
            DrawSimpleForce(_rightHand, _rightHandActive, Color.blue);
        }

        private void DrawSimpleForce(Rigidbody hand, bool active, Color color)
        {
            if (hand == null) return;

            bool isLeftHand = (hand == _leftHand);
            Vector3 handEnd = isLeftHand ? _leftEnd.position : _rightEnd.position;

            // Точка приложения силы
            Gizmos.color = color;
            Gizmos.DrawSphere(handEnd, 0.03f);

            // Направление силы
            if (active)
            {
                // К камере
                float sideOffset = 0.05f;
                Vector3 sideDirection = isLeftHand ? -_cameraPivot.right : _cameraPivot.right;
                Vector3 offsetTarget = _cameraPivot.position + sideDirection * sideOffset;

                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(handEnd, offsetTarget);
            }
            else
            {
                // Вниз
                Gizmos.color = Color.red;
                Gizmos.DrawRay(handEnd, Vector3.down * 0.3f);
            }
        }

    }
}