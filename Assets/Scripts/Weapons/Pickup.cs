using UnityEngine;
using Unity.Netcode;
using Unity.VisualScripting;

public class Pickup : NetworkBehaviour
{
    public WeaponPickup weapon;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        weapon = GetComponent<WeaponPickup>();

        if(weapon == null)
        {
            gameObject.GetComponent<NetworkObject>().Despawn();
            Debug.Log("despawn");
            return;
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void DieServerRpc()
    {
		gameObject.GetComponent<NetworkObject>().Despawn();
	}
}
