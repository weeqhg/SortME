using Unity.Netcode;
using UnityEngine;

public class ConveyorSend : NetworkBehaviour
{
    [SerializeField] private RackManager _rackManager;

    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("Item"))
        {
            MeshRenderer meshRenderer = other.GetComponent<MeshRenderer>();
            if (meshRenderer != null) meshRenderer.enabled = false;

            if (!IsServer) return;
            
            Rigidbody rb = other.attachedRigidbody;
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            _rackManager.PostItem(other.gameObject);
        }

    }
}
