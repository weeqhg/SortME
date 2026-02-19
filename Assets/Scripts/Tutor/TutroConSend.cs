using System.Collections;
using UnityEngine;
public class TutroConSend : MonoBehaviour
{
    [SerializeField] private AudioClip[] _clips;
    public AudioSource _audioSource;
    public Transform spawnPos;
    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Item"))
        {
            _audioSource.PlayOneShot(_clips[Random.Range(0, _clips.Length)]);
            StartCoroutine(DelayedServerLogic(other));
            TutorItemManager itemManager = other.GetComponent<TutorItemManager>();
            itemManager.UnpacItem();
            itemManager.ChangeUnpackState(true);
            itemManager.ChangeUnpackState(true);
            itemManager.info.ChangeBoxState(false);
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
        other.transform.position = spawnPos.position;
    }
}
