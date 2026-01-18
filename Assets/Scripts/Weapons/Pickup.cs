using UnityEngine;
using Unity.Netcode;
using Unity.VisualScripting;

public class Pickup : NetworkBehaviour
{
    public Weapon weapon;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void DieServerRpc()
    {
		gameObject.GetComponent<NetworkObject>().Despawn(true);
	}

    [ClientRpc]
    public void SetWeaponClientRpc(int index)
    {
        weapon = WeaponDataBase.GetWeaponById(index);
    }
}
