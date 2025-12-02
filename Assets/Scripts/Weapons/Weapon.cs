using UnityEngine;

[CreateAssetMenu(fileName = "Weapon", menuName = "Scriptable Objects/Weapon")]
public class Weapon : ScriptableObject
{
    [SerializeField] private Mesh weaponMesh;
    [SerializeField] private Projectile projectilePrefab;

    [SerializeField] private float damage = 1f;
    [SerializeField] private float strength = 1f;
    [SerializeField] private float range = 1f;
    [SerializeField] private float speed = 1f;
    [SerializeField] private float fireRate = 1f;

    public void Awake()
    {
        projectilePrefab.SetStats(speed, strength, range);
    }
    public void Shoot(Transform shootPoint, Vector3 direction)
    {
        if(projectilePrefab == null)
        {
            Debug.LogWarning("No projectile prefab assigned to weapon " + name);
            return;
        }

        projectilePrefab.GetComponent<Projectile>().direction = direction;
        Projectile projectile = Instantiate(projectilePrefab, shootPoint.position + direction * 1.1f, Quaternion.identity);
    }

}
