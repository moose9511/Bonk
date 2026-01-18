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
        Weapon weaponToSpawn = WeaponDataBase.GetRandomWeapon();
        if(weaponToSpawn != null)
        {
            Vector3 spawnPosition = transform.position + spawnOffset;

            pickupInstance = Instantiate(weaponPickupPrefab, spawnPosition, Quaternion.identity);
            pickupInstance.GetComponent<NetworkObject>().Spawn(true);

            var pickup = pickupInstance.GetComponent<Pickup>();

            if (pickup != null)
            {
                pickupInstance.GetComponent<Pickup>().SetWeaponClientRpc(weaponToSpawn.weaponId);
            }
        }
    }
}
