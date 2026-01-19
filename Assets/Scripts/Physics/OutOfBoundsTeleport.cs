using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class OutOfBoundsTeleport : MonoBehaviour
{
    [SerializeField] private Vector3 teleportPosition;
    [SerializeField] private bool kill;
    bool canTeleport = true;

    private void OnTriggerEnter(Collider other)
    {
        //if (!other.CompareTag("Player")) return;

        //var netObj = other.GetComponent<NetworkObject>();
        //if (!canTeleport) return;
        //StartCoroutine(TeleportCooldown());

        //if (kill)
        //    other.GetComponent<Player>().TakeDamageServerRpc(100000);
        //else
        //    other.gameObject.transform.position = teleportPosition;
    }

    public IEnumerator TeleportCooldown()
    {
        canTeleport = false;
        yield return new WaitForSeconds(0.5f); // small buffer
        canTeleport = true;
    }


}
