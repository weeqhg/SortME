using System;
using UnityEngine;

public enum ItemState
{
    Arrived,
    Stored,
    Dispatched,
    Ordering
}
public class ItemInfo : MonoBehaviour
{
    public string nameKeyItem;
    public Sprite icon;
    public Sprite box;
    public event Action<ItemInfo, ItemState> stateChanged;
    private ItemState _state;
    private bool isBox = true;
    private bool isChange = false;

    public bool IsBox
    {
        get => isBox;
    }
    public ItemState state
    {
        get => _state;
    }

    public void Reset()
    {
        isChange = false;
    }

    public void ChangeItemState(ItemState newState)
    {
        _state = newState;
        stateChanged?.Invoke(this, _state);
    }
    public void ChangeBoxState(bool value)
    {
        isBox = value;
    }
    public void ChangeNameItem(string newName)
    {
        if (isChange == false)
        {
            nameKeyItem = newName;
            isChange = true;
        }
    }
}
