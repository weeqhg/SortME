using UnityEngine;

namespace WekenDev.Player.Controller
{

    public class ControllerHands : MonoBehaviour
    {
        [Header("Ссылки")]
        [SerializeField] private Rigidbody _playerBody;
        [SerializeField] private Rigidbody _leftHand;
        [SerializeField] private Rigidbody _rightHand;
        [SerializeField] private Transform _cameraPivot;
        [SerializeField] private Transform _armsCenterPoint;
        [SerializeField] private Transform _leftHandTip; // Пустой GameObject на конце левой руки
        [SerializeField] private Transform _rightHandTip; // Пустой GameObject на конце правой руки
        //Подъем рук
        [SerializeField] private float _handForce = 100f; //Сила подъема рук
        private float _dragForce = 5f;

        //Стабилизация при ходьбе
        [SerializeField] private float downForce = 15f;
        [SerializeField] private float returnForce = 1f;
        [SerializeField] private float _maxArmDistance = 0.5f;

        // Цели для кончиков рук
        private Vector3 _leftTipTarget;
        private Vector3 _rightTipTarget;

        // Позиции покоя рук
        private Vector3 _leftHandRestPosition;
        private Vector3 _rightHandRestPosition;

        //Ввод игрока
    
        private bool _leftHandActive;
        private bool _rightHandActive;


        public void GetInput(bool left, bool right)
        {
            _leftHandActive = left;
            _rightHandActive = right;
        }

        private void Update()
        {
            // Обновляем позиции покоя (они двигаются с телом)
            UpdateRestPositions();

            // Обновляем целевые позиции
            UpdateTipTargets();
        }

        private void UpdateRestPositions()
        {
            // Позиции покоя - руки свисают по бокам тела
            _leftHandRestPosition = _playerBody.position +
                                  _playerBody.transform.right * -0.5f +
                                  Vector3.down * 1f;

            _rightHandRestPosition = _playerBody.position +
                                   _playerBody.transform.right * 0.5f +
                                   Vector3.down * 1f;
        }

        private void UpdateTipTargets()
        {
            // ЛЕВАЯ РУКА
            if (_leftHandActive)
            {
                // Вычисляем целевую позицию
                _leftTipTarget = _cameraPivot.position;
            }
            else
            {
                // Неактивная рука: цель = позиция покоя
                _leftTipTarget = _leftHandRestPosition;


            }

            // ПРАВАЯ РУКА
            if (_rightHandActive)
            {
                // Вычисляем целевую позицию
                _rightTipTarget = _cameraPivot.position;
            }
            else
            {
                // Неактивная рука: цель = позиция покоя
                _rightTipTarget = _rightHandRestPosition;
            }
        }

        private void FixedUpdate()
        {
            // ОБРАБОТКА ЛЕВОЙ РУКИ
            if (_leftHand != null)
            {
                if (_leftHandActive)
                {
                    // Активная рука: следим за целью
                    MoveHandToTarget(_leftHand, _leftTipTarget);
                }
                else
                {
                    // Неактивная рука: стабилизируемся
                    StabilizeHand(_leftHand, _leftHandRestPosition, false);
                }

                // Ограничение расстояния
                LimitHandDistance(_leftHand);
            }

            // ОБРАБОТКА ПРАВОЙ РУКИ
            if (_rightHand != null)
            {
                if (_rightHandActive)
                {
                    // Активная рука: следим за целью
                    MoveHandToTarget(_rightHand, _rightTipTarget);
                }
                else
                {
                    // Неактивная рука: стабилизируемся
                    StabilizeHand(_rightHand, _rightHandRestPosition, false);
                }

                // Ограничение расстояния
                LimitHandDistance(_rightHand);
            }
        }

        private void MoveHandToTarget(Rigidbody hand, Vector3 targetTipPosition)
        {
            Transform handTip = (hand == _leftHand) ? _leftHandTip : _rightHandTip;

            // 1. Определяем смещение в сторону от центра
            float sideOffset = 0.15f; // Расстояние от центра
            bool isLeftHand = (hand == _leftHand);

            // 2. Смещаем цель в сторону от центра cameraPivot
            Vector3 sideDirection = isLeftHand ? -_cameraPivot.right : _cameraPivot.right;
            Vector3 offsetTarget = targetTipPosition + sideDirection * sideOffset;

            // 3. Вычисляем позицию центра руки
            Vector3 tipToHand = hand.position - handTip.position;
            Vector3 targetHandPosition = offsetTarget + tipToHand;

            // 4. Сила к нужной позиции центра
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

            // 5. Демпфирование
            hand.AddForce(-hand.linearVelocity * _dragForce);

        }

        private void StabilizeHand(Rigidbody hand, Vector3 restPosition, bool isActiveHand)
        {
            // Только для неактивных рук
            if (isActiveHand) return;

            // 1. Притяжение к позиции покоя
            Vector3 toRest = restPosition - hand.position;
            float distanceToRest = toRest.magnitude;

            if (distanceToRest > 0.1f)
            {
                float restForce = Mathf.Clamp(distanceToRest * returnForce, 0f, returnForce);
                hand.AddForce(toRest.normalized * restForce * Time.fixedDeltaTime * 50f);
            }

            // 2. Мягкая сила вниз
            hand.AddForce(Vector3.down * downForce * Time.fixedDeltaTime * 50f);

            // 3. Сильное демпфирование для неактивных рук
            hand.AddForce(-hand.linearVelocity * _dragForce * 2f);

            // 4. Стабилизация вращения (руки свисают вниз)
            StabilizeHandRotation(hand, restPosition + Vector3.down * 0.5f);
        }

        private void StabilizeHandRotation(Rigidbody hand, Vector3 lookAtPosition)
        {
            // Направление взгляда руки
            Vector3 lookDirection = (lookAtPosition - hand.position).normalized;
            if (lookDirection == Vector3.zero) lookDirection = Vector3.down;

            // Целевое вращение
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection, Vector3.forward);
            Quaternion rotationDelta = targetRotation * Quaternion.Inverse(hand.rotation);

            rotationDelta.ToAngleAxis(out float angle, out Vector3 axis);

            // Нормализуем угол
            if (angle > 180f) angle -= 360f;

            // Применяем плавное вращение
            if (Mathf.Abs(angle) > 0.5f)
            {
                float torqueMultiplier = Mathf.Clamp(angle * 0.02f, -2f, 2f);
                Vector3 torque = axis * (torqueMultiplier * Mathf.Deg2Rad);
                hand.AddTorque(torque);
            }

            // Демпфирование вращения
            hand.AddTorque(-hand.angularVelocity * 2f);
        }

        private void LimitHandDistance(Rigidbody hand)
        {
            if (_playerBody == null) return;

            Vector3 toHand = hand.position - _armsCenterPoint.position;
            float distance = toHand.magnitude;

            if (distance > _maxArmDistance)
            {
                Vector3 pushBack = -toHand.normalized * returnForce * 2f * (distance - _maxArmDistance);
                hand.AddForce(pushBack * Time.fixedDeltaTime * 50f);
            }
        }

        private void OnDrawGizmos()
        {
            if (_playerBody == null) return;

            // Область досягаемости
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(_armsCenterPoint.position, _maxArmDistance);

            // Отладочные линии для рук
            if (_leftHand != null)
            {
                // Линия от конца левой руки к цели
                Vector3 leftHandTip = (_leftHandTip != null) ? _leftHandTip.position :
                     _leftHand.position + _leftHand.transform.forward * 0.5f;

                if (_leftHandActive && _cameraPivot != null)
                {
                    // Активная рука: линия к cameraPivot
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(leftHandTip, _cameraPivot.position);

                    // Точка цели
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(_cameraPivot.position, 0.07f);
                }
                else
                {
                    // Неактивная рука: линия к позиции покоя
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawLine(leftHandTip, _leftHandRestPosition);
                }

                // Визуализация длины руки
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(_leftHand.position, leftHandTip);
                Gizmos.DrawSphere(leftHandTip, 0.03f);
            }

            if (_rightHand != null)
            {
                // Линия от конца правой руки к цели
                Vector3 rightHandTip = (_rightHandTip != null) ? _rightHandTip.position :
                       _rightHand.position + _rightHand.transform.forward * 0.5f;

                if (_rightHandActive && _cameraPivot != null)
                {
                    // Активная рука: линия к cameraPivot
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(rightHandTip, _cameraPivot.position);

                    // Точка цели
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(_cameraPivot.position, 0.07f);
                }
                else
                {
                    // Неактивная рука: линия к позиции покоя
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawLine(rightHandTip, _rightHandRestPosition);
                }

                // Визуализация длины руки
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(_rightHand.position, rightHandTip);
                Gizmos.DrawSphere(rightHandTip, 0.03f);
            }

            // Линия от игрока к cameraPivot
            if (_cameraPivot != null)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(_playerBody.position, _cameraPivot.position);

                // Направление вперед от cameraPivot
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(_cameraPivot.position, _cameraPivot.forward * 0.5f);
            }
        }
    }
}