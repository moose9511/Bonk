using UnityEngine;
using Unity.Netcode;

public class Pickup : NetworkBehaviour
{
    public NetworkObject contents;

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!IsServer) return;

        Collider[] colliders = Physics.OverlapSphere(transform.position, 1f);

        foreach (Collider col in colliders)
        {
            Player player = col.GetComponent<Player>();
            if (player != null)
            {
                // spawns weapon
				NetworkObject spawnedWeapon = Instantiate(contents, player.transform);
                spawnedWeapon.Spawn();

                // gives player the weapon
                player.GiveWeapon();

                // destroys pickup
                spawnedWeapon.Despawn();
                gameObject.GetComponent<NetworkObject>().Despawn();
            }
        }
    }

    public void AddWeapon(NetworkObject weapon)
    {
        contents = weapon;
    }
}
