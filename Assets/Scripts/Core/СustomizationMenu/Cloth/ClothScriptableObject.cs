using UnityEngine;

public enum ClothType
{
    Accessories,
    Jacket
}

[CreateAssetMenu(fileName = "NewClothData", menuName = "Custom Data/ClothData")]
public class ClothScriptableObject : ScriptableObject
{
    public int id;
    public Mesh mesh;
    public Sprite icon;
    public ClothType type;
}
