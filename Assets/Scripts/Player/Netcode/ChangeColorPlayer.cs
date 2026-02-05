using UnityEngine;
using Unity.Netcode;

public class ChangeColorPlayer : NetworkBehaviour
{
    [SerializeField] private SkinnedMeshRenderer _body;
    [SerializeField] private Material _originalMaterial;
    private Material _clonedMaterial;
    private NetworkVariable<Color> _networkColor = new NetworkVariable<Color>(Color.white);

    public void Init()
    {
        _clonedMaterial = new(_originalMaterial);

        _body.material = _clonedMaterial;

        _networkColor.OnValueChanged += OnColorChanged;

        if (IsOwner)
        {
            LoadAndSendColor();
        }
        else
        {
            OnColorChanged(default, _networkColor.Value);
        }
    }

    private void LoadAndSendColor()
    {
        string savedColorHex = PlayerPrefs.GetString("PlayerColor", "FFFFFF");

        if (ColorUtility.TryParseHtmlString("#" + savedColorHex, out Color savedColor))
        {
            _clonedMaterial.color = savedColor;

            SendColorToServerRpc(savedColor);
        }
        else
        {
            SendColorToServerRpc(Color.white);
        }
    }

    [ServerRpc]
    private void SendColorToServerRpc(Color color)
    {
        _networkColor.Value = color;

        Debug.Log($"Сервер установил цвет игрока {OwnerClientId}: {color}");
    }

    private void OnColorChanged(Color oldColor, Color newColor)
    {
        // Этот метод вызывается на ВСЕХ клиентах при изменении NetworkVariable
        if (_clonedMaterial != null)
        {
            _clonedMaterial.color = newColor;
            Debug.Log($"Клиент {NetworkManager.LocalClientId} получил цвет: {newColor}");
        }
    }

    public override void OnNetworkDespawn()
    {
        // Отписываемся от события
        _networkColor.OnValueChanged -= OnColorChanged;

        // Очищаем материалы
        if (_clonedMaterial != null)
        {
            Destroy(_clonedMaterial);
        }
    }
}
