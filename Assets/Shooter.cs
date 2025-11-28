using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class Shooter : NetworkBehaviour
{
    public GameObject projectilePrefab;
	// Start is called once before the first execution of Update after the MonoBehaviour is created
	public override void OnNetworkSpawn()
    {
        StartCoroutine(shoot());
        projectilePrefab.GetComponent<Projectile>().SetStats(5f, transform.right, 5f, .5f);
	}

    private IEnumerator shoot() {
        yield return new WaitForSeconds(2f);
        Instantiate(projectilePrefab);
	}
	// Update is called once per frame
	void Update()
    {
        
    }
}
