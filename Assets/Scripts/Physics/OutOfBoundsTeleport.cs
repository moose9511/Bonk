using UnityEngine;

public class OutOfBoundsTeleport : MonoBehaviour
{
    [SerializeField] private Vector3 teleportPosition;
    [SerializeField] private bool kill;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("oh yes oh yes");
            if(kill)
                other.gameObject.GetComponent<Player>().TakeDamageServerRpc(100);
            else
                other.gameObject.transform.position = teleportPosition;
        }
    }
}
