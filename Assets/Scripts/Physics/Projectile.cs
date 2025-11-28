using UnityEngine;
using Unity.Netcode;

public class Projectile : NetworkBehaviour
{
    private float speed = 10f;

    public Vector3 direction;
    public float power;
    public float radius;

    Collider[] colliders;
	public override void OnNetworkSpawn()
    {

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
        transform.Translate(direction * speed * Time.deltaTime);
        colliders = Physics.OverlapSphere(transform.position, radius);

        foreach (Collider hit in colliders)
        {
            if (hit != null && hit.gameObject.CompareTag("Player"))
            {
                hit.gameObject.GetComponent<PlayerMovement2>() ? .AddForce(direction * power);
                Destroy(gameObject);
			}
		}
	}
}
