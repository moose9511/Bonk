using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
	[SerializeField] private Weapon weaponData; // reference to ScriptableObject

	public Weapon WeaponData => weaponData;
}
