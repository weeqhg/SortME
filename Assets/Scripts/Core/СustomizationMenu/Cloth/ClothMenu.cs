using System;
using UnityEngine;

public class ClothMenu : MonoBehaviour
{
    [SerializeField] private MeshFilter _accessoriesMesh;
    [SerializeField] private SkinnedMeshRenderer _jacket;
    [SerializeField] private ClothMenuUI _accessoriesUI;
    [SerializeField] private ClothMenuUI _jacketsUI;

    private ClothScriptableObject[] _accessories;
    private ClothScriptableObject[] _jackets;

    public void Init()
    {
        _accessories = Resources.LoadAll<ClothScriptableObject>("ClothsData/Accessories");
        _jackets = Resources.LoadAll<ClothScriptableObject>("ClothsData/Jackets");
        
        _accessoriesUI.Init(_accessories);

        _jacketsUI.Init(_jackets);

        _accessoriesUI.OnClothIconClicked += HandleAccessoriesForPlayer;
        _jacketsUI.OnClothIconClicked += HandleJacketForPlayer;

        Load();
    }

    private void Load()
    {
        int savedAccessoriesId = PlayerPrefs.GetInt("AccessoriesPlayer", 0);
        int savedJacketId = PlayerPrefs.GetInt("JacketPlayer", 0);

        _accessoriesMesh.mesh = ValidateClothId(savedAccessoriesId, _accessories);
        if (_jacket != null) _jacket.sharedMesh = ValidateClothId(savedJacketId, _jackets);
    }
    private Mesh ValidateClothId(int savedId, ClothScriptableObject[] clothArray)
    {
        if (savedId == 0) return null;

        foreach (ClothScriptableObject cloth in clothArray)
        {
            if (cloth != null && cloth.id == savedId)
            {
                return cloth.mesh;
            }
        }
        return null;
    }
    private void HandleAccessoriesForPlayer(int index)
    {
        if (_accessories != null && index < _accessories.Length)
        {
            ClothScriptableObject cloth = _accessories[index];
            _accessoriesMesh.mesh = cloth.mesh;

            int id = cloth.id;

            PlayerPrefs.SetInt("AccessoriesPlayer", id);
        }
    }

    private void HandleJacketForPlayer(int index)
    {
        if (_jackets != null && index < _jackets.Length)
        {
            ClothScriptableObject cloth = _jackets[index];
            _jacket.sharedMesh = cloth.mesh;

            int id = cloth.id;

            PlayerPrefs.SetInt("JacketPlayer", id);
        }
    }

    private void OnDestroy()
    {
        _accessoriesUI.OnClothIconClicked -= HandleAccessoriesForPlayer;
        _jacketsUI.OnClothIconClicked -= HandleJacketForPlayer;
    }


}