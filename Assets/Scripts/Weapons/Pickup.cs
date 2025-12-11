using UnityEngine;
using Unity.Netcode;
using Unity.VisualScripting;

public class Pickup : NetworkBehaviour
{
    public GameObject weapon;

    private Weapon weaponScript;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        weaponScript = GetComponent<Weapon>();

        if(weapon == null)
        {
            gameObject.GetComponent<NetworkObject>().Despawn();
            Debug.Log("despawn");
        }

        weaponScript.GetComponent<NetworkObject>().Spawn();
        weaponScript.OnNetworkSpawn();

    }
    // Update is called once per frame
    void FixedUpdate()
    {
        if (!IsServer) return;

        Collider[] colliders = Physics.OverlapSphere(transform.position, 1f);

        foreach (Collider col in colliders)
        {
            Player player = col.GetComponent<Player>();
            if (player == null) continue;

            Instantiate(weapon);

            // gives player the weapon
            player.GiveWeapon(weapon);

            gameObject.GetComponent<NetworkObject>().Despawn();
        }
    }
}
