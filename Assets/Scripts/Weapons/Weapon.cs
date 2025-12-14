using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "Weapon", menuName = "Scriptable Objects/Weapon")]
public class Weapon : ScriptableObject
{
	[SerializeField] public Mesh weaponMesh;
	[SerializeField] public GameObject projPrefab;

	[SerializeField] public float damage = 1f;
	[SerializeField] public float strength = 1f;
	[SerializeField] public float speed = 1f;
	[SerializeField] public float fireRate = 1f;
	[SerializeField] public float lifeTime = 10f;
	[SerializeField] public float radius = 1f;

	public int weaponId;
	public float distanceToShooter = 1.1f;

	public float[] SerializeData(Vector3 direction)
	{
		return new float[] { damage, strength, speed, lifeTime, radius, direction.x, direction.y, direction.z };
	}
}
