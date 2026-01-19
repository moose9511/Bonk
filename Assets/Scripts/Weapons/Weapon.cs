using UnityEngine;

public class Weapon : MonoBehaviour
{
	[SerializeField] public Mesh weaponMesh;
	[SerializeField] public GameObject projPrefab;

	[SerializeField] public float damage = 1f;
	[SerializeField] public float strength = 1f;
	[SerializeField] public float speed = 1f;
	[SerializeField] public float fireRate = 1f;
	[SerializeField] public float lifeTime = 10f;
	[SerializeField] public float radius = 1f;
	[SerializeField] public int ammo = 10;
	[SerializeField] public bool isAutomatic = false;

	public int weaponId;
	public int clientOwnerId;
	public float distanceToShooter = 1.1f;

	public float[] SerializeData(Vector3 direction)
	{
		return new float[] { damage, strength, speed, lifeTime, radius, direction.x, direction.y, direction.z, this.clientOwnerId};
	}
}
