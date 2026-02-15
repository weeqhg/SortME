using UnityEngine;
using Unity.Netcode;

namespace WekenDev.Player.Controller
{
    public class HandGrab : NetworkBehaviour
    {
        [SerializeField] private ControllerHands _myControllerHands;
        [Header("Настройки игрока")]
        [SerializeField] private Transform _cameraPivot;
        [Header("Синхронизация с прыжком")]
        [SerializeField] private Rigidbody _playerBody;
        [SerializeField] private Rigidbody _handRb;
        [Header("Настройки")]
        [SerializeField] private LayerMask _grabLayer = ~0;
        [SerializeField] private float _grabRadiusStatic = 0.01f;
        [SerializeField] private float _grabRadiusDynamic = 0.1f;
        [SerializeField] private float _grabForce = 200f;
        [SerializeField] private float _maxDistance = 0.5f;
        private float _jumpForce = 2f;
        private bool _isAttached = false;
        private bool _isGrabbing = false;

        private Rigidbody _objectRb;
        private Transform _attachedTransform;
        private Vector3 _attachLocalPoint;
        private float _lastHeightDiff;
        private float _lastCheckTime;
        private ConfigurableJoint _bodyJoint;
        private GameObject _anchorObject;
        private FixedJoint _fixedJoint;
        private bool _isUp = false;
        public void ToggleUp(bool enable)
        {
            _isUp = enable;
        }

        public void TryGrabObject()
        {
            if (!IsServer) return;

            if (_isAttached || _isGrabbing) return;

            TryGrabDynamic();

            TryGrabStatic();
        }

        private void FixedUpdate()
        {
            if (!IsServer) return;


            if (_isAttached)
            {
                UpdateAttachedPosition();
            }
        }

        private void UpdateAttachedPosition()
        {
            if (_attachedTransform == null || _handRb == null) return;

            Vector3 attachPoint = _attachedTransform.TransformPoint(_attachLocalPoint);
            float distance = Vector3.Distance(_handRb.position, attachPoint);

            if (distance > _maxDistance)
            {
                ReleaseObject();
                return;
            }

            if (_cameraPivot != null)
            {
                float cameraHeight = _cameraPivot.position.y;
                float playerHeight = _playerBody.position.y;
                float currentDiff = _cameraPivot.position.y - _playerBody.position.y;
                float timePassed = Time.time - _lastCheckTime;

                if (Mathf.Abs(currentDiff - _lastHeightDiff) * 10f > 0.5f && timePassed < 0.6f && cameraHeight < playerHeight + 0.3f) // Камера на 0.5м ниже игрока
                {
                    ReleaseObject();

                    Vector3 lift = _playerBody.transform.up * _jumpForce * _playerBody.mass;
                    _playerBody.AddForce(lift, ForceMode.Impulse);
                    return;
                }

                _lastHeightDiff = currentDiff;
                _lastCheckTime = Time.time;
            }

            _handRb.angularVelocity *= 0.8f;
            _playerBody.angularVelocity *= 0.5f;

        }

        private void TryGrabStatic()
        {
            Collider[] cols = Physics.OverlapSphere(transform.position, _grabRadiusStatic, _grabLayer);

            foreach (Collider col in cols)
            {
                if (col.transform.IsChildOf(_playerBody.transform)) continue;


                Rigidbody rb = col.GetComponent<Rigidbody>();
                if (rb == null || rb.isKinematic)
                {
                    AttachToObject(col.transform);

                    return;
                }
            }
        }

        private void AttachToObject(Transform targetTransform)
        {
            _isAttached = true;
            _attachedTransform = targetTransform;

            Vector3 attachWorldPoint = GetClosestPoint(targetTransform);
            _attachLocalPoint = targetTransform.InverseTransformPoint(attachWorldPoint);

            CreateAnchor(attachWorldPoint);

            CreateBodyJoint();
        }
        private Vector3 GetClosestPoint(Transform targetTransform)
        {
            Ray ray = new Ray(transform.position, transform.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 0.5f))
            {
                return hit.point;
            }

            Collider col = targetTransform.GetComponent<Collider>();
            if (col != null)
            {
                return col.ClosestPoint(transform.position);
            }

            return transform.position;
        }

        private void CreateAnchor(Vector3 worldPosition)
        {
            _anchorObject = new GameObject("AttachmentAnchor");
            _anchorObject.transform.position = worldPosition;
            _anchorObject.transform.SetParent(_attachedTransform);

            Rigidbody anchorRb = _anchorObject.AddComponent<Rigidbody>();
            anchorRb.isKinematic = true;
        }

        private void CreateBodyJoint()
        {
            // Создаем ConfigurableJoint на теле игрока
            _bodyJoint = _playerBody.gameObject.AddComponent<ConfigurableJoint>();
            _bodyJoint.connectedBody = _anchorObject.GetComponent<Rigidbody>();

            // Настройка линейных ограничений
            _bodyJoint.xMotion = ConfigurableJointMotion.Limited;
            _bodyJoint.yMotion = ConfigurableJointMotion.Limited;
            _bodyJoint.zMotion = ConfigurableJointMotion.Limited;

            // Маленький лимит для жесткого соединения
            SoftJointLimit limit = new SoftJointLimit();
            limit.limit = 10f;
            _bodyJoint.linearLimit = limit;
            // Настройка жесткости (пружины)
            JointDrive drive = new JointDrive();
            drive.positionSpring = 1000f;     // Жесткость
            drive.positionDamper = 50f;      // Демпфирование
            drive.maximumForce = 2000f;      // Макс. сила

            _bodyJoint.xDrive = drive;
            _bodyJoint.yDrive = drive;
            _bodyJoint.zDrive = drive;

            // Фиксируем вращение
            _bodyJoint.angularXMotion = ConfigurableJointMotion.Limited;
            _bodyJoint.angularYMotion = ConfigurableJointMotion.Limited;
            _bodyJoint.angularZMotion = ConfigurableJointMotion.Limited;

            SoftJointLimit angularLimit = new SoftJointLimit();
            angularLimit.limit = 30f; // Небольшой люфт

            _bodyJoint.angularYLimit = angularLimit;
            _bodyJoint.angularZLimit = angularLimit;
        }



        private void TryGrabDynamic()
        {
            Collider[] cols = Physics.OverlapSphere(transform.position, _grabRadiusDynamic, _grabLayer);

            foreach (Collider col in cols)
            {
                if (col.transform.IsChildOf(_playerBody.transform)) continue;
                if (_isUp && col.CompareTag("Player")) continue;

                Rigidbody rb = col.GetComponent<Rigidbody>();
                if (rb != null && !rb.isKinematic)
                {
                    if (col.CompareTag("Player"))
                    {
                        ControllerHands controllerHands = rb.GetComponentInParent<ControllerHands>();
                        controllerHands.OnUp();
                        _myControllerHands.otherHands = controllerHands;
                    }

                    CreateDynamicAnchor(rb);

                    _isGrabbing = true;
                    return;
                }
            }
        }

        private void CreateDynamicAnchor(Rigidbody targetRb)
        {
            // Добавляем FixedJoint к объекту
            _fixedJoint = targetRb.gameObject.AddComponent<FixedJoint>();
            _fixedJoint.connectedBody = _handRb;
            _fixedJoint.enableCollision = false;
            _fixedJoint.breakForce = 1000f;
        }

        public void ReleaseObject()
        {
            if (!_isGrabbing && !_isAttached) return;


            if (_isGrabbing)
            {
                if (_fixedJoint != null) Destroy(_fixedJoint);

                _isGrabbing = false;
            }
            else if (_isAttached)
            {
                if (_bodyJoint != null)
                {
                    Destroy(_bodyJoint);
                    _bodyJoint = null;
                }

                if (_anchorObject != null)
                {
                    Destroy(_anchorObject);
                    _anchorObject = null;
                }

                _isAttached = false;
                _attachedTransform = null;
            }
        }
    }
}
