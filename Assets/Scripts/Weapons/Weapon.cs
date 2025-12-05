using System.Collections;
using UnityEngine;
using Unity.Netcode;
using Unity.VisualScripting;

[CreateAssetMenu(fileName = "Weapon", menuName = "Scriptable Objects/Weapon")]
public class Weapon : ScriptableObject
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
        GameObject obj = Instantiate(projPrefab, shootPoint.position + direction, Quaternion.identity);
        obj.AddComponent<Projectile>();

        if(obj == null)
        {
            Debug.Log("how");
            return;
        }
        obj.GetComponent<Projectile>().Init(this, direction);
    }

}

class Projectile : NetworkBehaviour
{
    private Collider[] colliders;
    private GameObject ground;

    private Weapon weapon;
    private Vector3 direction;
    private IEnumerator die()
    {
        yield return new WaitForSeconds(weapon.lifeTime);
        Destroy(gameObject);
    }
    public void Init(Weapon weapon, Vector3 direction)
    {
        this.weapon = weapon;
        this.direction = direction;
        ground.layer = LayerMask.NameToLayer("Ground");

        if(weapon == null)
        {
            Debug.Log("weapon is null");
            Destroy(gameObject);
        }

        StartCoroutine(die());
    }
    void Update()
    {
        transform.Translate(weapon.speed * Time.deltaTime * direction);
        colliders = Physics.OverlapSphere(transform.position, weapon.radius);
        foreach (Collider hit in colliders)
        {
            if (hit != null)
            {
                Debug.Log(LayerMask.Equals(hit.gameObject.layer, ground.layer) + " | " + hit.gameObject.layer + " | " + ground.layer);
                if (hit.gameObject.CompareTag("Player"))
                {
                    hit.gameObject.GetComponent<PlayerMovement2>()?.AddForce(direction * weapon.strength);
                    hit.gameObject.GetComponent<Player>()? .TakeDamage(weapon.damage);
                    Destroy(gameObject);
                } else if (LayerMask.Equals(hit.gameObject.layer, ground.layer))
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}
