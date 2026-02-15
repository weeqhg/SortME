using TMPro;
using Unity.Netcode;
using UnityEngine;

public class Rack : NetworkBehaviour
{
    [SerializeField] private RackManager _rackManager;
    [SerializeField] private string _id;
    public Transform spawnPos;
    private TextMeshProUGUI[] _textTable;
    private ItemManager _item;
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

    public ItemManager GetIDandItem()
    {
        return _item;
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
        if (!IsServer) return;

        if (other.CompareTag("Item"))
        {
            ItemInfo info = other.GetComponent<ItemInfo>();
            if (info.state != ItemState.Ordering)
            {
                ItemManager itemManager = other.GetComponent<ItemManager>();
                MeshRenderer meshRenderer = other.GetComponent<MeshRenderer>();
                if (meshRenderer != null) meshRenderer.enabled = true;

                itemManager.UnpacItem();
                itemManager.ChangeItemState(ItemState.Stored);
                itemManager.ChangeNameItem(_id);
                itemManager.OnDestroyItem += OnDestroyItem;
                _item = itemManager;
            }
        }
    }
    private void OnDestroyItem()
    {
        _item = null;
    }
    private void OnTriggerExit(Collider other)
    {
        if (!IsServer) return;

        if (other.CompareTag("Item"))
        {
            ItemManager itemManager = other.GetComponent<ItemManager>();
            itemManager?.OutRack();
            itemManager.OnDestroyItem -= OnDestroyItem;
            _item = null;
        }
    }


}
