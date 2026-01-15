using UnityEngine;

public class OutOfBoundsTeleport : MonoBehaviour
{
    [SerializeField] private Vector3 teleportPosition;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            other.gameObject.transform.position = teleportPosition;
        }
    }
}
