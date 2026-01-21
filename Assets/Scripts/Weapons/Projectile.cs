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

    // for client use
	private Vector3 lastPos;
    private bool isVisual = false;
    private float speed;
    private ulong networkId;

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
		lastPos = transform.position;

		StartCoroutine(die());
    }

    public void Init(float speed, Vector3 position, Vector3 direction, ulong networkId)
    {
        isVisual = true;
        transform.position = position;
        this.direction = direction;
        this.speed = speed;
        this.networkId = networkId;
    }

    void Update()
    {
        if(IsServer && !isVisual)
		    ServerUpdate();
        else if (isVisual)
            ClientUpdate();
    }

    private void ServerUpdate()
    {
		if (values == null || values.Length == 0)
			return;

        lastPos = transform.position;
        transform.Translate(values[2] * Time.deltaTime * direction);

		float distance = Vector3.Distance(lastPos, transform.position);

		Physics.SphereCast(transform.position, values[4], direction, out RaycastHit hit, distance);
		if (hit.collider == null) return;

		if (hit.collider.gameObject.CompareTag("Player") && hit.collider.gameObject.GetComponent<NetworkObject>().OwnerClientId != values[8])
		{
			hit.collider.GetComponent<PlayerMovement2>()?.AddForceRpc(direction * values[1]);
			hit.collider.GetComponent<Player>()?.TakeDamageServerRpc(values[0]);
			networkObject.Despawn();
		}
		else if (hit.collider.gameObject.layer == Player.groundLayer)
		{
			if (GetComponent<NetworkObject>().IsSpawned)
				GetComponent<NetworkObject>().Despawn();
		}
    }

    private void ClientUpdate()
    {
        transform.Translate(speed * Time.deltaTime * direction);
        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.ContainsKey(networkId))
        {
            Destroy(gameObject);
            return;
        }

    }
            

    private void OnDrawGizmos()
    {
		if(values != null)
            Gizmos.DrawWireSphere(transform.position, values[4]);
    }

}
