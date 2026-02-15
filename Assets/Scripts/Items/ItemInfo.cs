using System;
using UnityEngine;

public enum ItemState
{
    Arrived,
    Stored,
    Ordering,
    Dispatched
}
public class ItemInfo : MonoBehaviour
{
    public string nameKeyItem;
    public Sprite icon;
    public Sprite box;
    private ItemState _state;
    private bool isBox = true;
    private bool isUnpack = false;

    public bool IsBox
    {
        get => isBox;
    }
    public bool IsUnpack
    {
        get => isUnpack;
    }
    public ItemState state
    {
        get => _state;
    }

    public void ChangeItemState(ItemState newState)
    {
        _state = newState;
    }
    public void ChangeBoxState(bool value)
    {
        isBox = value;
    }
    public void ChangeUnpackState(bool value)
    {
        isUnpack = value;
    }
    public void ChangeNameItem(string newName)
    {
        if (ItemState.Ordering != state)
        {
            nameKeyItem = newName;
        }
    }
}
