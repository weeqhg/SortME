using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization.Components;
using Unity.Netcode;
using System.Collections.Generic;
using Unity.Collections;

public class OrderUI : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private Image _wait;
    [SerializeField] private Image _icon;
    [SerializeField] private LocalizeStringEvent _nameGate;

    [SerializeField] private CanvasGroup _numOrder;
    [SerializeField] private CanvasGroup _order;
    [SerializeField] private CanvasGroup _numGate;

    [SerializeField] private List<Sprite> _availableIcons;
    private NetworkVariable<int> _iconIndex = new NetworkVariable<int>(0);
    private NetworkVariable<float> _waitProgress = new NetworkVariable<float>(0f);
    private NetworkVariable<FixedString32Bytes> _itemId = new NetworkVariable<FixedString32Bytes>();

    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _completeOrder;
    [SerializeField] private AudioClip _failOrder;
    [SerializeField] private AudioClip _newOrder;
    [SerializeField] private AudioClip _gateOrder;
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
        _numGate.alpha = 0f;
        _order.alpha = 0f;
        _numOrder.alpha = 0f;
    }

    [ClientRpc]
    public void ChangeStateOnWaitClientRpc()
    {
        _order.alpha = 0f;
        _numGate.alpha = 0f;
        _numOrder.alpha = 0f;
    }

    [ClientRpc]
    public void ChangeStateOnCompleteClientRpc()
    {
        _order.alpha = 0f;
        _numGate.alpha = 0f;
        _numOrder.alpha = 0f;

        _audioSource.PlayOneShot(_completeOrder);
    }

    [ClientRpc]
    public void ChangeStateOnFailClientRpc()
    {
        _order.alpha = 0f;
        _numGate.alpha = 0f;
        _numOrder.alpha = 0f;

        _audioSource.PlayOneShot(_failOrder);
    }

    [ClientRpc]
    private void ChangeStateOrderClientRpc()
    {
        _numOrder.alpha = 1f;
        _order.alpha = 1f;
        _numGate.alpha = 0f;

        _audioSource.PlayOneShot(_newOrder);
    }

    [ClientRpc]
    private void ChangeStateGateClientRpc(int index)
    {
        _numOrder.alpha = 0f;
        _order.alpha = 1f;
        _numGate.alpha = 1f;

        _nameGate.StringReference.Arguments = new object[] { index + 1 };
        _nameGate.RefreshString();

        _audioSource.PlayOneShot(_gateOrder);
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
