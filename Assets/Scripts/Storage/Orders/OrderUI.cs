using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class OrderUI : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private Image _wait;
    [SerializeField] private Image _icon;
    [SerializeField] private LocalizeStringEvent _nameGate;

    [SerializeField] private CanvasGroup _numOrder;
    [SerializeField] private CanvasGroup _order;
    [SerializeField] private CanvasGroup _numGate;

    [SerializeField] private CanvasGroup _nonItem;
    [SerializeField] private CanvasGroup _searchOrder;

    [SerializeField] private List<Sprite> _availableIcons;
    private NetworkVariable<int> _iconIndex = new NetworkVariable<int>(0);
    private NetworkVariable<float> _waitProgress = new NetworkVariable<float>(0f);
    private NetworkVariable<FixedString32Bytes> _itemId = new NetworkVariable<FixedString32Bytes>();

    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _completeOrder;
    [SerializeField] private AudioClip _failOrder;
    [SerializeField] private AudioClip _newOrder;
    [SerializeField] private AudioClip _gateOrder;


    [Header("Rating (5 stars)")]
    [SerializeField] private CanvasGroup _ratingGroup;
    [SerializeField] private Image[] _ratingStars = new Image[5];
    [SerializeField] private Sprite _starOn;
    [SerializeField] private Sprite _starOff;
    [SerializeField] private float _ratingShowDuration = 2f;

    private Coroutine _ratingHideRoutine;
    public override void OnNetworkSpawn()
    {
        _waitProgress.OnValueChanged += OnProgressChanged;
        _iconIndex.OnValueChanged += OnIconIndexChanged;
        _itemId.OnValueChanged += OnItemNameChanged;

        if (_availableIcons == null || _availableIcons.Count == 0)
        {
            Sprite[] sprites = Resources.LoadAll<Sprite>("ItemData/Icons"); // папка Resources/Icons
            _availableIcons = new List<Sprite>(sprites);
        }
    }
    public void Init()
    {
        _nonItem.alpha = 1f;
        _searchOrder.alpha = 0f;
        _numGate.alpha = 0f;
        _order.alpha = 0f;
        _numOrder.alpha = 0f;
    }

    [ClientRpc]
    public void ChangeStateOnWaitClientRpc()
    {
        _nonItem.alpha = 1f;
        _searchOrder.alpha = 0f;
        _order.alpha = 0f;
        _numGate.alpha = 0f;
        _numOrder.alpha = 0f;
    }

    [ClientRpc]
    public void ChangeStateOnCompleteClientRpc()
    {
        _nonItem.alpha = 0f;
        _searchOrder.alpha = 0f;
        _order.alpha = 0f;
        _numGate.alpha = 0f;
        _numOrder.alpha = 0f;

        _audioSource.PlayOneShot(_completeOrder);
    }

    [ClientRpc]
    public void ChangeStateOnFailClientRpc()
    {
        _nonItem.alpha = 0f;
        _searchOrder.alpha = 0f;
        _order.alpha = 0f;
        _numGate.alpha = 0f;
        _numOrder.alpha = 0f;

        _audioSource.PlayOneShot(_failOrder);
    }

    [ClientRpc]
    private void ChangeStateOrderClientRpc()
    {
        _nonItem.alpha = 0f;
        _searchOrder.alpha = 0f;
        _numOrder.alpha = 1f;
        _order.alpha = 1f;
        _numGate.alpha = 0f;

        _audioSource.PlayOneShot(_newOrder);
    }

    [ClientRpc]
    private void ChangeStateGateClientRpc(int index)
    {
        _nonItem.alpha = 0f;
        _searchOrder.alpha = 0f;
        _numOrder.alpha = 0f;
        _order.alpha = 1f;
        _numGate.alpha = 1f;

        _nameGate.StringReference.Arguments = new object[] { index + 1 };
        _nameGate.RefreshString();

        _audioSource.PlayOneShot(_gateOrder);
    }

    public void ShowRatingLocal(int stars)
    {
        int clamped = Mathf.Clamp(stars, 0, 5);

        if (_ratingStars != null && _ratingStars.Length > 0)
        {
            for (int i = 0; i < _ratingStars.Length; i++)
            {
                if (_ratingStars[i] == null) continue;
                if (_starOn != null && _starOff != null)
                    _ratingStars[i].sprite = i < clamped ? _starOn : _starOff;
                else
                    _ratingStars[i].color = i < clamped ? Color.yellow : new Color(1f, 1f, 1f, 0.25f);
            }
        }

        if (_ratingGroup != null)
        {
            if (_ratingHideRoutine != null) StopCoroutine(_ratingHideRoutine);
            _ratingGroup.alpha = 1f;
            _ratingHideRoutine = StartCoroutine(HideRatingAfterDelay(_ratingShowDuration));
        }
    }

    private IEnumerator HideRatingAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (_ratingGroup == null)
        {
            if (_ratingStars != null)
            {
                for (int i = 0; i < _ratingStars.Length; i++)
                {
                    if (_ratingStars[i] == null) continue;
                    if (_starOff != null) _ratingStars[i].sprite = _starOff;
                    else _ratingStars[i].color = new Color(1f, 1f, 1f, 0.25f);
                }
            }
            _ratingHideRoutine = null;
            yield break;
        }

        float t = 0f;
        float fade = 0.25f;
        float start = _ratingGroup.alpha;
        while (t < fade)
        {
            t += Time.deltaTime;
            _ratingGroup.alpha = Mathf.Lerp(start, 0f, t / fade);
            yield return null;
        }

        _ratingGroup.alpha = 0f;

        _searchOrder.alpha = 1f;

        _ratingHideRoutine = null;
    }

    public void WaitTimer(float value)
    {
        if (!IsServer) return;
        _waitProgress.Value = Mathf.Clamp01(value);
    }

    public void UpdateOrder(string name, Sprite icon)
    {
        ChangeStateOrderClientRpc();

        int id = _availableIcons.IndexOf(icon);
        _iconIndex.Value = id >= 0 ? id : 0;

        _itemId.Value = name;
    }

    public void UpdateGate(int index, Sprite icon)
    {
        ChangeStateGateClientRpc(index);

        int id = _availableIcons.IndexOf(icon);
        _iconIndex.Value = id >= 0 ? id : 0;
    }



    private void OnItemNameChanged(FixedString32Bytes oldValue, FixedString32Bytes newValue)
    {
        _text.text = newValue.ToString();
    }

    private void OnProgressChanged(float oldValue, float newValue)
    {
        _wait.fillAmount = newValue;
    }

    private void OnIconIndexChanged(int oldValue, int newValue)
    {
        if (newValue >= 0 && newValue < _availableIcons.Count)
        {
            _icon.sprite = _availableIcons[newValue];
        }
    }

    public override void OnNetworkDespawn()
    {
        _waitProgress.OnValueChanged -= OnProgressChanged;
        _iconIndex.OnValueChanged -= OnIconIndexChanged;
        _itemId.OnValueChanged -= OnItemNameChanged;
    }
}
