using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System.Linq;

public class ContainerManager : MonoBehaviour
{
    [SerializeField] private GameObject[] _itemsPrefab;
    [SerializeField] private Transform[] _spawnPos;
    private List<GameObject> _spawnedItems = new List<GameObject>();


    //Это должен выполнять только сервер
    public void SpawnItems(int count)
    {
        if (_itemsPrefab.Length < 0 && _spawnPos.Length < 0) return;

        for (int i = 0; i < count; i++)
        {
            GameObject itemToReuse = _spawnedItems.FirstOrDefault(item =>
{
    ItemInfo info = item.GetComponent<ItemInfo>();
    return info != null && info.state == ItemState.Dispatched;
});

            if (itemToReuse != null)
            {
                Transform pos = _spawnPos[Random.Range(0, _spawnPos.Length)];
                itemToReuse.transform.position = pos.position;
                itemToReuse.SetActive(true);
                ItemInfo info = itemToReuse.GetComponent<ItemInfo>();
                info.state = ItemState.Arrived;

                NetworkObject netObj = itemToReuse.GetComponent<NetworkObject>();
                if (netObj != null && !netObj.IsSpawned)
                    netObj.Spawn();
            }
            else
            {
                GameObject prefab = _itemsPrefab[Random.Range(0, _itemsPrefab.Length)];
                Transform pos = _spawnPos[Random.Range(0, _spawnPos.Length)];

                GameObject obj = Instantiate(prefab, pos);
                _spawnedItems.Add(obj);
                ItemInfo info = itemToReuse.GetComponent<ItemInfo>();
                info.state = ItemState.Arrived;

                NetworkObject netObj = obj.GetComponent<NetworkObject>();
                if (netObj != null)
                    netObj.Spawn();
            }
        }
    }
}
