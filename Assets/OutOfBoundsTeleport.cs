using UnityEngine;

public class OutOfBoundsTeleport : MonoBehaviour
{
    [SerializeField] private Vector3 teleportPosition;
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("yep1");
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("yep2");
            other.gameObject.transform.position = teleportPosition;
        }
    }
}
