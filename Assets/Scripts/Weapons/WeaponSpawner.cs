using UnityEngine;
using Unity.Netcode;

public class WeaponSpawner : NetworkBehaviour
{
    [SerializeField] private Vector3 spawnOffset;
    [SerializeField] private GameObject weaponPickupPrefab;

    [SerializeField] private float spawnInterval = 10f;
    private float timer;

    private bool hasSpawned = false;
    private GameObject pickupInstance;

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
        {
            enabled = false;
            return;
        }

        base.OnNetworkSpawn();
        if(weaponPickupPrefab == null)
        {
            Debug.Log("Weapon pickup prefab is not assigned in WeaponSpawner.");
            enabled = false;
            return;
        }

        timer = spawnInterval;

    }

    private void FixedUpdate()
    {
        if (!IsServer) return;
        if(pickupInstance == null && hasSpawned == true)
        {
            hasSpawned = false;
        }

        if(timer >= spawnInterval && !hasSpawned)
        {
            if (NetworkManager.Singleton == null) return;
            hasSpawned = true;
            SpawnWeaponServerRpc();
            timer = 0f;
        }
        else if(!hasSpawned)
        {
            timer += Time.deltaTime;
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void SpawnWeaponServerRpc()
    {
        float pickupType = Random.value;

        Vector3 spawnPosition = transform.position + spawnOffset;

        pickupInstance = Instantiate(weaponPickupPrefab, spawnPosition, Quaternion.identity);
        pickupInstance.GetComponent<NetworkObject>().Spawn(true);

        var pickup = pickupInstance.GetComponent<Pickup>();

        if (pickupType < .01f/*.65*/)
        {
            pickup.EnableWeaponClientRpc();
            Weapon weaponToSpawn = WeaponDataBase.GetRandomWeapon();

            if (weaponToSpawn != null)
            {
                pickupInstance.GetComponent<Pickup>().SetWeaponClientRpc(weaponToSpawn.weaponId);
                pickup.SetMaterialClientRpc(TextureManager.comGunInd);
            }
        }
        else if (pickupType < .01f/*.83*/)
        {
            pickup.EnableHealthClientRpc();
            float health = Random.value;
            if(health < .6)
            {
                pickup.SetHealthClientRpc(15f);
                pickup.SetMaterialClientRpc(TextureManager.health1Ind);
            } else if (health < .9)
            {
                pickup.SetHealthClientRpc(30f);
                pickup.SetMaterialClientRpc(TextureManager.health2Ind);
            } else
            {
                pickup.SetHealthClientRpc(50f);
                pickup.SetMaterialClientRpc(TextureManager.health3Ind);
            }
        } else
        {
            pickup.EnablePowerClientRpc();
            pickup.SetMaterialClientRpc(TextureManager.powerInd);
        }
        
    }
}
