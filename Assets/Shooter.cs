using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class Shooter : NetworkBehaviour
{
    public GameObject projectilePrefab;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        StartCoroutine(shoot());
        projectilePrefab.GetComponent<Projectile>().SetStats(5f, transform.right, 5f, .5f);
    }
    public override void OnNetworkSpawn()
    {
       
    }

    private IEnumerator shoot() {
        yield return new WaitForSeconds(2f);
        Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        StartCoroutine(shoot());
    }
	// Update is called once per frame
	void Update()
    {
        
    }
}
