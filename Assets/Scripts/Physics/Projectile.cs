using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class Projectile : NetworkBehaviour
{
    private float speed = 10f;

    public Vector3 direction;
    public float power;
    public float radius;
    public float lifetime = 15f;

    Collider[] colliders;

    private static LayerMask ground;
    private IEnumerator die()
    {
        yield return new WaitForSeconds(lifetime);
        Destroy(gameObject);
    }

    public override void OnNetworkSpawn()
    {
        ground = LayerMask.NameToLayer("Ground");
        StartCoroutine(die());
    }

    public void SetStats(float projSpeed, float projPower, float projRadius)
    {
        speed = projSpeed;
        power = projPower;
        radius = projRadius;
    }

    public void SetStats(float projSpeed, Vector3 projDirection, float projPower, float projRadius)
    {
        speed = projSpeed;
        direction = projDirection;
        power = projPower;
        radius = projRadius;
    }
	void Update()
    {
        transform.Translate(speed * Time.deltaTime * direction);
        colliders = Physics.OverlapSphere(transform.position, radius);

        foreach (Collider hit in colliders)
        {
            if (hit != null)
            {
                if (hit.gameObject.CompareTag("Player"))
                {
                    hit.gameObject.GetComponent<PlayerMovement2>() ? .AddForce(direction * power);
                    Destroy(gameObject);
                } else if (hit.gameObject.layer == ground)
                {
                    Destroy(gameObject);
                }
            }
		}
	}
}
