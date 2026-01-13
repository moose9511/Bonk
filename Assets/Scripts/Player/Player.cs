using System.Runtime.CompilerServices;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Player : NetworkBehaviour
{
	// health text
	[SerializeField] private GameObject canvas;

	[SerializeField] private TextMeshProUGUI healthText;

    [SerializeField] private GameObject cam;

	[SerializeField] private LayerMask pickupMask;

	public static int groundLayer;

	// health
    private NetworkVariable<float> health = new(
		100f, NetworkVariableReadPermission.Owner, NetworkVariableWritePermission.Server);
	private NetworkVariable<int> weaponId = new(
		-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

	private Weapon currentWeapon = null;

    public override void OnNetworkSpawn()
    {
		// sets ground layer
        groundLayer = LayerMask.NameToLayer("Ground");

		// sets correct canvas for each player
		if (!IsOwner) { 
			canvas.SetActive(false);
		}
		else
		{
			health.OnValueChanged += OnHealthChanged;
			canvas.SetActive(true);
		}
    }

	private void OnHealthChanged(float oldValue, float newValue)
	{
		if (IsOwner)
			healthText.text = newValue.ToString("0");
	}

	[Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
	public void TakeDamageServerRpc(float damage)
	{
		health.Value = Mathf.Max(0, health.Value - damage);
	}

	[Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
	public void GainHealthServerRpc(float heal)
	{
		health.Value = Mathf.Min(100f, health.Value + heal);
	}

	public void Update()
    {
		if(!IsOwner) return;

		// checks to see if player has collided with a pickup
		Collider[] pickups = Physics.OverlapCapsule(transform.position + new Vector3(0, .5f, 0), transform.position - new Vector3(0, .5f, 0), .6f, pickupMask);
		foreach (Collider coll in pickups)
		{
			Pickup pickup = coll.gameObject.GetComponent<Pickup>();

			WeaponPickup weaponPickup = coll.gameObject.GetComponent<WeaponPickup>();
			if (weaponPickup != null)
			{
				currentWeapon = weaponPickup.WeaponData;
				SetWeaponServerRpc(currentWeapon.weaponId);
			}

            pickup.DieServerRpc();
		}

		
		if (currentWeapon != null && Input.GetMouseButtonDown(0))
        {
			float[] values = currentWeapon.SerializeData(cam.transform.forward);
            SpawnProjServerRpc(values);
        }
    }


	[Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
	public void SpawnProjServerRpc(float[] values)
	{
        var projObj = Instantiate(currentWeapon.projPrefab,
        cam.transform.position + cam.transform.forward * currentWeapon.distanceToShooter,
        Quaternion.identity);

        var proj = projObj.GetComponent<Projectile>();
        proj.GetComponent<NetworkObject>().Spawn(true);
		
		proj.Init(values);
	}

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
	public void SetWeaponServerRpc(int weaponId)
	{
		currentWeapon = WeaponDataBase.GetWeaponById(weaponId);
	}

	
}

