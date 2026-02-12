using Unity.Netcode;
using UnityEngine;

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
            MeshRenderer meshRenderer = other.GetComponent<MeshRenderer>();
            if (meshRenderer != null) meshRenderer.enabled = false;
            if (_audioSource != null) _audioSource.PlayOneShot(_clips[Random.Range(0, _clips.Length)]);

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
