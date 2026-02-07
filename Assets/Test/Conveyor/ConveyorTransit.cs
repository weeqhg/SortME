using UnityEngine;

public class ConveyorTransit : MonoBehaviour
{
    [SerializeField] private Transform _startPoint;
    [SerializeField] private Transform _endPoint;
    [SerializeField] private float _speed = 1f;

    private void OnTriggerStay(Collider other)
    {
        Vector3 direction = (_endPoint.position - _startPoint.position).normalized;
        other.attachedRigidbody.AddForce(direction * _speed, ForceMode.Force);
    }
}