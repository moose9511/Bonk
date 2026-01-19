using UnityEngine;
using Unity.Netcode;
using Unity.VisualScripting;

public class Pickup : NetworkBehaviour
{ 
    public Weapon weapon;
    public float healthGained;
    public int powerIndex = -1;

    public bool isWeapon = false;
    public bool isHealth = false;
    public bool isPower = false;
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
    [ClientRpc]
    public void SetHealthClientRpc(float amout)
    {
        healthGained = amout;
    }
    [ClientRpc]
    public void SetPowerIndexClientRpc(int index)
    {
        powerIndex = index;
    }


    [ClientRpc] 
    public void EnableWeaponClientRpc()
    {
        isWeapon = true;
    }
    [ClientRpc]
    public void EnableHealthClientRpc()
    {
        isHealth = true;
    }
    [ClientRpc]
    public void EnablePowerClientRpc()
    {
        isPower = true;
    }
    [ClientRpc]
    public void SetMaterialClientRpc(int matIndex)
    {
        if (TextureManager.textures[matIndex] == null) Debug.LogError("texture null");
        GetComponent<Renderer>().material = TextureManager.textures[matIndex];  
    }
}
