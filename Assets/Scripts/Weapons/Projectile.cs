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
     * */
    private float[] values;

    private Vector3 direction;
    private NetworkObject networkObject;
    private IEnumerator die()
    {
        yield return new WaitForSeconds(values[3]);
        Destroy(gameObject);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void InitServerRpc(float[] values)
    {
		this.values = values;
        direction = new Vector3(values[5], values[6], values[7]);

		networkObject = GetComponent<NetworkObject>();

		StartCoroutine(die());
    }

    void Update()
    {
        if(values == null || values.Length == 0) return;

        transform.Translate(values[2] * Time.deltaTime * direction);
        colliders = Physics.OverlapSphere(transform.position, values[4]);
        foreach (Collider hit in colliders)
        {
            if (hit.gameObject.CompareTag("Player"))
            {
                hit.GetComponent<PlayerMovement2>()?.AddForce(direction * values[1]);
                hit.GetComponent<Player>()?.TakeDamageServerRpc(values[0]);
                networkObject.Despawn();
            }
            else if (hit.gameObject.layer == Player.groundLayer)
            {
                GetComponent<NetworkObject>().Despawn();
            }
        }
    }

}
