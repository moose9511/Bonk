using UnityEngine;

public class TeleportUp : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.transform.position = new Vector3(0, 10, 0);
            Debug.Log("Player Teleported Up");
        }
    }
}
