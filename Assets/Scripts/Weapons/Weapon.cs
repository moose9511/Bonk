using System.Collections;
using UnityEngine;
using Unity.Netcode;
using Unity.VisualScripting;

public class Weapon : NetworkBehaviour
{
    [SerializeField] private Mesh weaponMesh;
    [SerializeField] private GameObject projPrefab;

    [SerializeField] public float damage = 1f;
    [SerializeField] public float strength = 1f;
    [SerializeField] public float range = 1f;
    [SerializeField] public float speed = 1f;
    [SerializeField] public float fireRate = 1f;
    [SerializeField] public float lifeTime = 10f;
    [SerializeField] public float radius = 1f;

    public void Shoot(Transform shootPoint, Vector3 direction)
    {
        GameObject obj = Instantiate(projPrefab, shootPoint.position + direction * 2f, Quaternion.identity);
        obj.AddComponent<Projectile>();

        if(obj == null)
            return;

        obj.GetComponent<Projectile>().Init(this, direction);
    }

}

class Projectile : NetworkBehaviour
{
    private Collider[] colliders;

    private Weapon weapon;
    private Vector3 direction;
    private LayerMask groundMask;
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
        groundMask = LayerMask.GetMask("Ground");

        networkObject = GetComponent<NetworkObject>();
        networkObject.Spawn();
        
        if(weapon == null)
        {
            Debug.Log("weapon is null");
            networkObject.Despawn();
        }

        StartCoroutine(die());
    }
    void Update()
    {
        transform.Translate(weapon.speed * Time.deltaTime * direction);
        colliders = Physics.OverlapSphere(transform.position, weapon.radius);
        foreach (Collider hit in colliders)
        {
            if (hit == null) continue;

            if (hit.gameObject.CompareTag("Player"))
            {
                hit.gameObject.GetComponent<PlayerMovement2>()?.AddForce(direction * weapon.strength);
                hit.gameObject.GetComponent<Player>()? .TakeDamageServerRpc(weapon.damage);
                networkObject.Despawn();
            } else if (hit.gameObject.layer == Player.groundLayer)
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
