using UnityEngine;

public enum ItemState
{
    Arrived,
    Stored,
    Dispatched,
    Lost        
}
public class ItemInfo : MonoBehaviour
{
    public int id;
    public Sprite icon;
    public ItemState state;
}
