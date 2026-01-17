using UnityEngine;
using Unity.Netcode;

public class WeaponSpawner : NetworkBehaviour
{
    [SerializeField] private Vector3 spawnOffset;
    [SerializeField] private GameObject weaponPickupPrefab;

    [SerializeField] private float spawnInterval = 10f;
    private float timer = 0f;

    private bool hasSpawned = false;

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

    }

    private void FixedUpdate()
    {
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

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    private void SpawnWeaponServerRpc()
    {
        Weapon weaponToSpawn = WeaponDataBase.GetRandomWeapon();
        if(weaponToSpawn != null)
        {
            Vector3 spawnPosition = transform.position + spawnOffset;
            GameObject weaponInstance = Instantiate(weaponPickupPrefab, spawnPosition, Quaternion.identity);
            weaponInstance.GetComponent<Pickup>().weapon = weaponToSpawn;
            weaponInstance.GetComponent<NetworkObject>().Spawn(true);
        }
    }
}
