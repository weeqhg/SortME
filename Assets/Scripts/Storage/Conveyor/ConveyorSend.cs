using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class ConveyorSend : NetworkBehaviour
{
    [SerializeField] private AudioClip[] _clips;
    [SerializeField] private RackManager _rackManager;
    public AudioSource _audioSource;
    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Item"))
        {
            OnItemCollectedClientRpc(other.gameObject);

            if (IsServer)
            {
                StartCoroutine(DelayedServerLogic(other));
            }
        }
    }


    [ClientRpc]
    private void OnItemCollectedClientRpc(NetworkObjectReference itemRef)
    {
        if (itemRef.TryGet(out NetworkObject netObj))
        {
            MeshRenderer meshRenderer = netObj.GetComponent<MeshRenderer>();
            if (meshRenderer != null) meshRenderer.enabled = false;

            if (_audioSource != null)
                _audioSource.PlayOneShot(_clips[Random.Range(0, _clips.Length)]);
        }
    }

    private IEnumerator DelayedServerLogic(Collider other)
    {
        // Ждем кадр, чтобы клиенты успели скрыть предмет
        yield return null;

        Rigidbody rb = other.attachedRigidbody;
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        _rackManager.PostItem(other.gameObject);
    }
}
