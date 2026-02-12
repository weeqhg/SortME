using UnityEngine;
using UnityEngine.Localization.Components;

[CreateAssetMenu(fileName = "NewItemData", menuName = "Custom Data/ItemData")]
public class ItemScriptableObject : ScriptableObject
{
    public string nameKeyItem;
    public Sprite icon;
    public Mesh mesh;
    public Texture texture;
}
