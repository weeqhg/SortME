using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class ChangeClothPlayer : NetworkBehaviour
{
    [SerializeField] private MeshFilter _accessories;
    [SerializeField] private SkinnedMeshRenderer _jacket;

    // NetworkVariable для синхронизации ID одежды
    private NetworkVariable<int> _networkHatId = new NetworkVariable<int>(0);
    private NetworkVariable<int> _networkJacketId = new NetworkVariable<int>(0);

    // Кэш для загруженных мешей
    private Dictionary<int, Mesh> _accessoriesCache = new Dictionary<int, Mesh>();
    private Dictionary<int, Mesh> _jacketsCache = new Dictionary<int, Mesh>();

    public void Init()
    {
        if (IsOwner)
        {
            if (_accessories != null) LoadCloth(ClothType.Accessories);
            if (_jacket != null) LoadCloth(ClothType.Jacket);
        }
        else
        {
            OnHatIdChanged(default, _networkHatId.Value);
            OnJacketIdChanged(default, _networkJacketId.Value);
        }

        _networkHatId.OnValueChanged += OnHatIdChanged;
        _networkJacketId.OnValueChanged += OnJacketIdChanged;
    }

    private void LoadCloth(ClothType type)
    {
        switch (type)
        {
            case ClothType.Accessories:
                int savedHatId = PlayerPrefs.GetInt("AccessoriesPlayer", 0);
                _accessories.mesh = LoadMeshFromCache("ClothsData/Accessories", savedHatId, _accessoriesCache);
                UpdateClothServerRpc(ClothType.Accessories, savedHatId);
                break;
            case ClothType.Jacket:
                int savedJacketId = PlayerPrefs.GetInt("JacketPlayer", 0);
                _jacket.sharedMesh = LoadMeshFromCache("ClothsData/Jackets", savedJacketId, _jacketsCache);
                UpdateClothServerRpc(ClothType.Jacket, savedJacketId);
                break;
        }
    }

    [ServerRpc]
    private void UpdateClothServerRpc(ClothType type, int clothId)
    {
        switch (type)
        {
            case ClothType.Accessories:
                _networkHatId.Value = clothId;
                break;
            case ClothType.Jacket:
                _networkJacketId.Value = clothId;
                break;
        }

        Debug.Log($"Сервер установил {type} ID {clothId} для игрока {OwnerClientId}");
    }

    private void OnHatIdChanged(int oldId, int newId)
    {
        Debug.Log($"Игрок {OwnerClientId} сменил шапку: {oldId} -> {newId}");

        if (_accessories != null)
        {
            Mesh accessoriesMesh = LoadMeshFromCache("ClothsData/Accessories", newId, _accessoriesCache);
            if (accessoriesMesh != null)
            {
                _accessories.mesh = accessoriesMesh;
            }
        }
    }

    private void OnJacketIdChanged(int oldId, int newId)
    {
        Debug.Log($"Игрок {OwnerClientId} сменил куртку: {oldId} -> {newId}");

        if (_jacket != null)
        {
            Mesh jacketMesh = LoadMeshFromCache("ClothsData/Jackets", newId, _jacketsCache);
            if (jacketMesh != null)
            {
                _jacket.sharedMesh = jacketMesh;
            }
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

    public override void OnNetworkDespawn()
    {
        // Отписываемся от событий
        if (_networkHatId != null)
            _networkHatId.OnValueChanged -= OnHatIdChanged;

        if (_networkJacketId != null)
            _networkJacketId.OnValueChanged -= OnJacketIdChanged;
    }
}