using Unity.Netcode;
using UnityEngine;
using DG.Tweening;

namespace WekenDev.Player.Controller
{

    public class PlayerController : NetworkBehaviour
    {
        [SerializeField] private Rigidbody _mainBody;
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip[] _footsteps;
        [SerializeField] private AudioClip _jumpClip;

        [Header("Leg Animation")]
        [SerializeField] private Transform _feetsPos;
        [SerializeField] private Transform _leftLeg;
        [SerializeField] private Transform _rightLeg;

        [Header("Movement")]
        [SerializeField] private float _speed = 5f;
        [SerializeField] private float _uprightStrength = 600f;
        [SerializeField] private Transform _cameraPivot;

        [Header("Jump")]
        [SerializeField] private float _jumpForce = 5f;
        [SerializeField] private LayerMask _groundLayer = ~0;

        // Запоминаем начальное вращение ног
        private Vector3 _startPosLeft;
        private Vector3 _startPosRight;

        private Vector3 _cachedForward;
        private Vector3 _cachedRight;

        //Ввод передвижения
        private Vector2 _moveInput;
        private bool _jumpInput;
        private bool _leftHand;
        private bool _rightHand;
        private NetworkVariable<bool> _netMove = new();

        public void Init()
        {
            _netMove.OnValueChanged += OnMoveStateChanged;

            _startPosLeft = _leftLeg.localPosition;

            _startPosRight = _rightLeg.localPosition;
        }

        public void GetInput(Vector2 moveInput, bool jump, bool leftHand, bool rightHand)
        {
            _moveInput = moveInput;
            _jumpInput = jump;
            _leftHand = leftHand;
            _rightHand = rightHand;
        }

        private void AudioEffect()
        {
            if (_audioSource == null && _footsteps.Length > 0) return;

            if (IsGrounded())
            {
                AudioClip randomClip = _footsteps[Random.Range(0, _footsteps.Length)];
                _audioSource.PlayOneShot(randomClip);
            }
        }

        private void OnMoveStateChanged(bool oldValue, bool newValue)
        {
            StartWalkAnimation();
        }
        private Sequence _walkSequence;
        private void StartWalkAnimation()
        {
            if (_netMove.Value)
            {
                // Останавливаем предыдущую анимацию
                if (_walkSequence != null)
                {
                    _walkSequence.Kill();
                    _walkSequence = null;
                }

                _leftLeg.localRotation = Quaternion.Euler(-80, 0, 0);
                _rightLeg.localRotation = Quaternion.Euler(-80, 0, 0);

                _walkSequence = DOTween.Sequence();

                _walkSequence.Append(_leftLeg.DOLocalRotate(new Vector3(-80, 90, 0), 0.2f).SetEase(Ease.OutQuad));
                _walkSequence.Join(_rightLeg.DOLocalRotate(new Vector3(-80, 90, 0), 0.2f).SetEase(Ease.OutQuad));
                _walkSequence.AppendCallback(() => AudioEffect());

                _walkSequence.Append(_leftLeg.DOLocalRotate(new Vector3(-80, -90, 0), 0.2f).SetEase(Ease.OutQuad));
                _walkSequence.Join(_rightLeg.DOLocalRotate(new Vector3(-80, -90, 0), 0.2f).SetEase(Ease.OutQuad));
                _walkSequence.AppendCallback(() => AudioEffect());

                _walkSequence.SetLoops(-1, LoopType.Restart);
                _walkSequence.Play();
            }
            else
            {
                if (_walkSequence != null)
                {
                    _walkSequence.Kill();
                    _walkSequence = null;
                }

                _leftLeg.DOLocalRotate(new Vector3(-80, 0, 0), 0.2f);
                _rightLeg.DOLocalRotate(new Vector3(-80, 0, 0), 0.2f);
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

                if (IsGrounded()) _mainBody.AddForce(Vector3.up * 15f, ForceMode.Impulse);
            }
            else
            {
                if (_netMove.Value) _netMove.Value = false;
            }

            // Прыжок
            if (_jumpInput && IsGrounded())
            {
                PlayJumpAnimationClientRpc();

                _mainBody.AddForce(Vector3.up * _jumpForce * 100f, ForceMode.Impulse);

                Vector3 horizontalVel = new Vector3(_mainBody.linearVelocity.x, 0, _mainBody.linearVelocity.z);
                _mainBody.AddForce(horizontalVel.normalized * 1.3f, ForceMode.VelocityChange);

                _jumpInput = false;
            }
        }
        [ClientRpc]
        private void PlayJumpAnimationClientRpc()
        {
            DOTween.Kill(_leftLeg);
            DOTween.Kill(_rightLeg);

            _audioSource.PlayOneShot(_jumpClip);

            Sequence jumpSequence = DOTween.Sequence();
            jumpSequence
                .Append(_leftLeg.DOLocalMoveZ(_startPosLeft.z - 0.3f, 0.1f).SetEase(Ease.OutQuad))
                .Join(_rightLeg.DOLocalMoveZ(_startPosRight.z - 0.3f, 0.1f).SetEase(Ease.OutQuad))
                .Append(_leftLeg.DOLocalMoveZ(_startPosLeft.z + 0.2f, 0.2f).SetEase(Ease.InOutSine))
                .Join(_rightLeg.DOLocalMoveZ(_startPosRight.z + 0.2f, 0.2f).SetEase(Ease.InOutSine))
                .Append(_leftLeg.DOLocalMoveZ(_startPosLeft.z, 0.15f).SetEase(Ease.OutBack))
                .Join(_rightLeg.DOLocalMoveZ(_startPosRight.z, 0.15f).SetEase(Ease.OutBack))
                .OnComplete(() =>
                {
                    if (_netMove.Value) StartWalkAnimation();
                });
        }

        private bool IsGrounded()
        {
            Vector3 start = _feetsPos.position;
            float _groundCheckRadius = 0.2f;
            float _groundCheckHeight = 0.6f;
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