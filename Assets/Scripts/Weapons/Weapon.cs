using UnityEngine;

[CreateAssetMenu(fileName = "Weapon", menuName = "Scriptable Objects/Weapon")]
public class Weapon : ScriptableObject
{
	[SerializeField] public Mesh weaponMesh;
	[SerializeField] public GameObject projPrefab;

	[SerializeField] public float damage = 1f;
	[SerializeField] public float strength = 1f;
	[SerializeField] public float range = 1f;
	[SerializeField] public float speed = 1f;
	[SerializeField] public float fireRate = 1f;
	[SerializeField] public float lifeTime = 10f;
	[SerializeField] public float radius = 1f;
}
