using TMPro;
using Unity.Netcode;
using UnityEngine;

public class Rack : MonoBehaviour
{
    [SerializeField] private RackManager _rackManager;
    [SerializeField] private string _id;
    public Transform spawnPos;
    private TextMeshProUGUI[] _textTable;
    [SerializeField] private ItemInfo _item;

    private void Start()
    {
        Init();
    }
    private void Init()
    {
        _id = gameObject.name;

        _textTable = GetComponentsInChildren<TextMeshProUGUI>();
        if (_textTable.Length > 0)
        {
            for (int i = 0; i < _textTable.Length; i++)
            {
                _textTable[i].text = _id;
            }
        }

        _rackManager.Register(this);
    }

    public (string, ItemInfo) GetIDandItem()
    {
        return (_id, _item);
    }

    public bool IsBusy()
    {
        if (_item == null) return false;
        else return true;
    }

    public void PlaceItem(GameObject itemPrefab)
    {
        itemPrefab.transform.position = spawnPos.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Item"))
        {
            _item = other.GetComponent<ItemInfo>();
            if (_item.state != ItemState.Dispatched)
            {
                MeshRenderer meshRenderer = other.GetComponent<MeshRenderer>();
                if (meshRenderer != null) meshRenderer.enabled = true;
                DamageableItem damageableItem = other.GetComponent<DamageableItem>();
                damageableItem.UnpacItem();
                _item.state = ItemState.Stored;

                Debug.Log(_item.name + "Размещён");
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Item"))
        {
            _item.state = ItemState.Lost;
            _item = null;
        }
    }


}
