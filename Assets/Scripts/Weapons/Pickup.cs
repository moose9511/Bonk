using UnityEngine;
using Unity.Netcode;

public class Pickup : NetworkBehaviour
{
    public Weapon contents;

    // Update is called once per frame
    void FixedUpdate()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, 1f);

        foreach (Collider col in colliders)
        {
            Player player = col.GetComponent<Player>();
            if (player != null && player.IsOwner)
            {
                player.GiveWeapon(contents);
                Destroy(gameObject);
            }
        }
    }

    public void AddWeapon(Weapon weapon)
    {
        contents = weapon;
    }
}
