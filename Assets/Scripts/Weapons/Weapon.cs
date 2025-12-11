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

    private Vector3 shootDirection;

    [ServerRpc]
    public void ShootServerRpc()
    {
        // gets facing direction
        shootDirection = GetComponentInChildren<Camera>().transform.forward;

        // spawns projectile in front of player
        GameObject obj = Instantiate(projPrefab, transform.position + shootDirection * 2f, Quaternion.identity);
        obj.GetComponent<NetworkObject>().Spawn(); 

        // initializes it's projectile script
        var proj = obj.GetComponent<Projectile>();
        proj.Init(this, shootDirection);
    }

}

