using System;
using Unity.Netcode;
using UnityEngine;

public class ContainerOrder : NetworkBehaviour
{

    [SerializeField] private GateOpenClose _gate;
    private NetworkVariable<bool> _netIsOpen = new NetworkVariable<bool>(false);
    [SerializeField] private OrderManager _orderManager;
    [SerializeField] private int _idContainer;
    public int GetId() => _idContainer - 1;
    private string _orderID = "";
    [SerializeField] private bool _hasCorrectItem = false;
    [SerializeField] private int _playerInside = 0;
    private ItemManager _item;
    [SerializeField] private Transform _posPush;
    public bool IsPlayer
    {
        get
        {
            return _playerInside > 0;
        }
    }

    public override void OnNetworkSpawn()
    {
        _netIsOpen.OnValueChanged += OnGateStateChanged;
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

    public void ChangeOrderID(string orderID)
    {
        _orderID = orderID;
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

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        if (other.CompareTag("Item"))
        {
            ItemInfo info = other.GetComponent<ItemInfo>();

            if (info != null && info.nameKeyItem == _orderID)
            {
                _item = other.GetComponent<ItemManager>();

                if (!IsServer) return;

                _hasCorrectItem = true;

                if (IsNearPlayer())
                {
                    StartCoroutine(WaitAndCheckAgain());
                }
                else
                {
                    CheckCloseContainer();
                }
            }
        }
        if (other.CompareTag("Player"))
        {
            _playerInside++;
        }
    }

    private System.Collections.IEnumerator WaitAndCheckAgain()
    {
        while (IsNearPlayer())
        {
            yield return new WaitForSeconds(2f);
        }

        CheckCloseContainer();
    }


    private System.Collections.IEnumerator CloseTemporarily()
    {
        yield return new WaitForSeconds(0.5f);
        Close();
        StartCoroutine(OpenAfterDelay(15f)); // или 20f
    }

    private System.Collections.IEnumerator OpenAfterDelay(float delay)
    {
        yield return new WaitForSeconds(0.5f);

        _item.GameObject.SetActive(false);
        _item.ChangeUnpackStateClientRpc(false);
        _item.ChangeItemState(ItemState.Dispatched);

        NetworkObject netObj = _item.GameObject.GetComponent<NetworkObject>();

        if (netObj != null && netObj.IsSpawned)
            netObj.Despawn(false);

        yield return new WaitForSeconds(delay);
        Open();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsServer) return;

        if (other.CompareTag("Item"))
        {
            ItemInfo item = other.GetComponent<ItemInfo>();

            if (item != null && item.nameKeyItem == _orderID)
            {
                _hasCorrectItem = false;
                _item = null;
            }
        }
        if (other.CompareTag("Player"))
        {
            _playerInside--;

            if (IsNearPlayer())
            {
                StartCoroutine(WaitAndCheckAgain());
            }
            else
            {
                CheckCloseContainer();
            }
        }
    }

    private void CheckCloseContainer()
    {
        if (_playerInside <= 0 && _hasCorrectItem)
        {
            StartCoroutine(CloseTemporarily());
            _orderManager.CompleteOrder();
            _orderID = "";
            _hasCorrectItem = false;
        }
    }
    private bool IsNearPlayer()
    {
        Collider[] players = Physics.OverlapSphere(_posPush.position, 3f);

        foreach (Collider col in players)
        {
            if (col.CompareTag("Player"))
            {
                return true;
            }
        }
        return false;
    }

    public override void OnNetworkDespawn()
    {
        _netIsOpen.OnValueChanged -= OnGateStateChanged;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(_posPush.position, 3f);
    }
}
