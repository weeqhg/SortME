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
    [SerializeField] private CanvasGroup _order;
    [SerializeField] private CanvasGroup _numGate;

    [SerializeField] private List<Sprite> _availableIcons;
    private NetworkVariable<int> _iconIndex = new NetworkVariable<int>(0);
    private NetworkVariable<int> _gateIndex = new NetworkVariable<int>(0);
    private NetworkVariable<float> _waitProgress = new NetworkVariable<float>(0f);
    private NetworkVariable<FixedString32Bytes> _itemId = new NetworkVariable<FixedString32Bytes>();
    public override void OnNetworkSpawn()
    {
        _waitProgress.OnValueChanged += OnProgressChanged;
        _gateIndex.OnValueChanged += OnIndexGateChanged;
        _iconIndex.OnValueChanged += OnIconIndexChanged;
        _itemId.OnValueChanged += OnItemNameChanged;
    }
    public void Init()
    {
        _numGate.alpha = 0f;
        _order.alpha = 0f;
        _text.text = "";
    }

    [ClientRpc]
    public void ChangeStateOnWaitClientRpc()
    {
        _order.alpha = 0f;
        _numGate.alpha = 0f;
        _text.text = "";
    }

    [ClientRpc]
    private void ChangeStateOrderClientRpc()
    {
        _order.alpha = 1f;
        _numGate.alpha = 0f;
    }

    [ClientRpc]
    private void ChangeStateGateClientRpc()
    {
        _order.alpha = 1f;
        _numGate.alpha = 1f;
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
        ChangeStateGateClientRpc();

        int id = _availableIcons.IndexOf(icon);
        _iconIndex.Value = id >= 0 ? id : 0;

        _gateIndex.Value = index;
    }



    private void OnItemNameChanged(FixedString32Bytes oldValue, FixedString32Bytes newValue)
    {
        _text.text = newValue.ToString();
    }

    private void OnIndexGateChanged(int oldValue, int newValue)
    {
        Debug.Log($"Gate changed: {newValue}");

        _nameGate.StringReference.Arguments = new object[] { newValue + 1 };
        _nameGate.RefreshString();
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
        _gateIndex.OnValueChanged -= OnIndexGateChanged;
        _iconIndex.OnValueChanged -= OnIconIndexChanged;
        _itemId.OnValueChanged -= OnItemNameChanged;
    }
}
