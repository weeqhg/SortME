using Unity.Netcode;
using UnityEngine;

public class ContainerDeliver : NetworkBehaviour
{
    [SerializeField] private GateOpenClose _gate;
    private NetworkVariable<bool> _netIsOpen = new NetworkVariable<bool>(false);
    [SerializeField] private ContainerManager _containerManager;
    [SerializeField] private int _idContainer;
    private int _playerInside = 0;
    private int _itemInside = 0;

    public bool IsAvalible
    {
        get
        {
            return _itemInside <= 0 && _playerInside <= 0;
        }
    }

    public override void OnNetworkSpawn()
    {
        _netIsOpen.OnValueChanged += OnGateStateChanged;
    }

    public void Open()
    {
        if (IsServer) _netIsOpen.Value = true;
        _gate.OpenGate();
    }

    public void Close()
    {
        if (IsServer) _netIsOpen.Value = false;
        _gate.CloseGate();
    }



    private void OnGateStateChanged(bool oldValue, bool newValue)
    {
        if (newValue)
        {
            Open();
        }
        else
        {
            Close();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        if (other.CompareTag("Item"))
        {
            _itemInside++;
        }
        if (other.CompareTag("Player"))
        {
            _playerInside++;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsServer) return;

        if (other.CompareTag("Item"))
        {
            _itemInside--;
            CheckCloseContainer();
        }
        if (other.CompareTag("Player"))
        {
            _playerInside--;
            CheckCloseContainer();
        }
    }

    private void CheckCloseContainer()
    {
        if (_playerInside <= 0 && _itemInside <= 0)
        {
            _containerManager.ContainerEmptied(_idContainer - 1);
            Close();
        }
    }

    public override void OnNetworkDespawn()
    {
        _netIsOpen.OnValueChanged -= OnGateStateChanged;
    }
}
