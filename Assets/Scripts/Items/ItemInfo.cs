using UnityEngine;
using UnityEngine.Localization.Components;

public enum ItemState
{
    Arrived,
    Stored,
    Dispatched,
    Lost        
}
public class ItemInfo : MonoBehaviour
{
    public string nameKeyItem;
    public Sprite icon;
    public ItemState state;
}
