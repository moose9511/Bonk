using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class Projectile : NetworkBehaviour
{
    private Collider[] colliders;

    /* list of values goes like this:
     * 0: damage
     * 1: strength
     * 2: speed
     * 3: lifetime
     * 4: radius
     * 5: x
     * 6: y
     * 7: z
     * 8: clientId
     * */
    private float[] values;

    private Vector3 direction;
    private NetworkObject networkObject;
    private IEnumerator die()
    {
        yield return new WaitForSeconds(values[3]);
        Destroy(gameObject);
    }

    //[Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void Init(float[] values)
    {
		this.values = values;
        direction = new Vector3(values[5], values[6], values[7]);

		networkObject = GetComponent<NetworkObject>();

		StartCoroutine(die());
    }

    void Update()
    {
        if (values == null || values.Length == 0)
            return;

        transform.Translate(values[2] * Time.deltaTime * direction);
        Physics.SphereCast(transform.position, values[4], direction, out RaycastHit hit, values[4]);
        if (hit.collider == null) return;

        if (hit.collider.gameObject.CompareTag("Player") && hit.collider.gameObject.GetComponent<NetworkObject>().OwnerClientId != values[8])
        {
            hit.collider.GetComponent<PlayerMovement2>()?.AddForceRpc(direction * values[1]);
            hit.collider.GetComponent<Player>()?.TakeDamageServerRpc(values[0]);
            networkObject.Despawn();
        }
        else if (hit.collider.gameObject.layer == Player.groundLayer)
        {
            if(GetComponent<NetworkObject>().IsSpawned)
                GetComponent<NetworkObject>().Despawn();
        }
    }

}
