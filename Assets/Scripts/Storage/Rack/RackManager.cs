using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RackManager : MonoBehaviour
{
    [SerializeField] private List<Rack> _racks;

    public void Register(Rack rack)
    {
        _racks.Add(rack);
    }

    public void PostItem(GameObject item)
    {
        if (_racks.Count < 0) return;

        var freeRacks = _racks.Where(r => !r.IsBusy()).ToArray();
        if (freeRacks.Length == 0) return;

        freeRacks[Random.Range(0, freeRacks.Length)].PlaceItem(item);
    }

    public ItemManager GetRandomRackIDandID()
    {
        var occupiedRacks = _racks.Where(r => r.IsBusy()).ToArray();
        if (occupiedRacks.Length == 0) return null;

        return occupiedRacks[Random.Range(0, occupiedRacks.Length)].GetIDandItem();
    }
}

