using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class Projectile : NetworkBehaviour
{
    private Collider[] colliders;

    private Weapon weapon;
    private Vector3 direction;
    private NetworkObject networkObject;
    private IEnumerator die()
    {
        yield return new WaitForSeconds(weapon.lifeTime);
        Destroy(gameObject);
    }

    public void Init(Weapon weapon, Vector3 direction)
    {
        this.weapon = weapon;
        this.direction = direction;
        networkObject = GetComponent<NetworkObject>();

        if (weapon == null)
        {
            Debug.Log("weapon is null");
            GetComponent<NetworkObject>().Despawn();
        }

        StartCoroutine(die());
    }
    void Update()
    {
        transform.Translate(weapon.speed * Time.deltaTime * direction);
        colliders = Physics.OverlapSphere(transform.position, weapon.radius);
        foreach (Collider hit in colliders)
        {
            if (hit.gameObject.CompareTag("Player"))
            {
                hit.GetComponent<PlayerMovement2>()?.AddForce(direction * weapon.strength);
                hit.GetComponent<Player>()?.TakeDamageServerRpc(weapon.damage);
                networkObject.Despawn();
            }
            else if (hit.gameObject.layer == Player.groundLayer)
            {
                GetComponent<NetworkObject>().Despawn();
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, weapon.radius);
    }
}
