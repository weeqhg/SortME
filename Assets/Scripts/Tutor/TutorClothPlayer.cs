using UnityEngine;
using System.Collections.Generic;


public class TutorClothPlayer : MonoBehaviour
{
    [SerializeField] private MeshFilter _accessories;
    [SerializeField] private SkinnedMeshRenderer _jacket;

    // Кэш для загруженных мешей
    private Dictionary<int, Mesh> _accessoriesCache = new Dictionary<int, Mesh>();
    private Dictionary<int, Mesh> _jacketsCache = new Dictionary<int, Mesh>();

    public void Init()
    {

            if (_accessories != null) LoadCloth(ClothType.Accessories);
            if (_jacket != null) LoadCloth(ClothType.Jacket);
        
    }

    private void LoadCloth(ClothType type)
    {
        switch (type)
        {
            case ClothType.Accessories:
                int savedHatId = PlayerPrefs.GetInt("AccessoriesPlayer", 0);
                _accessories.mesh = LoadMeshFromCache("ClothsData/Accessories", savedHatId, _accessoriesCache);

                break;
            case ClothType.Jacket:
                int savedJacketId = PlayerPrefs.GetInt("JacketPlayer", 0);
                _jacket.sharedMesh = LoadMeshFromCache("ClothsData/Jackets", savedJacketId, _jacketsCache);

                break;
        }
    }




  

    private Mesh LoadMeshFromCache(string path, int clothId, Dictionary<int, Mesh> cache)
    {
        if (cache.TryGetValue(clothId, out Mesh cachedMesh))
        {
            return cachedMesh;
        }

        // Если нет в кэше, загружаем
        ClothScriptableObject cloth = GetClothById(path, clothId);
        if (cloth != null && cloth.mesh != null)
        {
            cache[clothId] = cloth.mesh;
            return cloth.mesh;
        }

        // Если не найден, загружаем дефолтный (ID = 0)
        if (clothId != 0)
        {
            return LoadMeshFromCache(path, 0, cache);
        }

        return null;
    }

    private ClothScriptableObject GetClothById(string path, int id)
    {
        ClothScriptableObject[] allCloths = Resources.LoadAll<ClothScriptableObject>(path);

        foreach (ClothScriptableObject cloth in allCloths)
        {
            if (cloth.id == id)
                return cloth;
        }

        Debug.LogWarning($"Cloth with ID {id} not found in {path}!");
        return null;
    }

 
}
