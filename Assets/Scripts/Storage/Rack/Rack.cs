using TMPro;
using UnityEngine;

public class Rack : MonoBehaviour
{
    [SerializeField] private RackManager _rackManager;
    [SerializeField] private string _id;
    public Transform spawnPos;
    private TextMeshProUGUI[] _textTable;
    [SerializeField] private ItemInfo _itemInfo;

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

    public ItemInfo GetIDandItem()
    {
        return _itemInfo;
    }

    public bool IsBusy()
    {
        if (_itemInfo == null) return false;
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
            _itemInfo = other.GetComponent<ItemInfo>();
            if (_itemInfo.state != ItemState.Dispatched)
            {
                MeshRenderer meshRenderer = other.GetComponent<MeshRenderer>();
                if (meshRenderer != null) meshRenderer.enabled = true;

                DurabilityItem damageableItem = other.GetComponent<DurabilityItem>();
                damageableItem.UnpacItem();

                _itemInfo.ChangeNameItem(_id);
                _itemInfo.ChangeItemState(ItemState.Stored);
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Item"))
        {
            _itemInfo.ChangeItemState(ItemState.Ordering);
            _itemInfo = null;
        }
    }


}
