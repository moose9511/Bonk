using TMPro;
using UnityEngine;
using Unity.Netcode;

public class Player : NetworkBehaviour
{
	// health text
    [SerializeField] private TextMeshProUGUI healthText;

    [SerializeField] private GameObject cam;

	// health
    private NetworkVariable<float> health = new(
		100f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

	// weapon reference
	private NetworkVariable<NetworkObject> weapon = new(
		null, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server); 

    public override void OnNetworkSpawn()
    {
		// sets onvaluechanged methods to network variables
        if (!IsOwner) return;
        health.OnValueChanged += OnHealthChanged;
		weapon.OnValueChanged += OnWeaponChanged;
    }

	private void OnHealthChanged(float oldValue, float newValue)
	{
		if (IsOwner)
			healthText.text = newValue.ToString("0");
	}
	private void OnWeaponChanged(NetworkObject oldValue, NetworkObject newValue)
	{
		if (IsOwner)
			weapon.Value = newValue;
	}

	[ServerRpc]
	public void TakeDamageServerRpc(float damage)
	{
		health.Value = Mathf.Max(0, health.Value - damage);
	}

	[ServerRpc]
	public void GainHealthServerRpc(float heal)
	{
		health.Value = Mathf.Min(100f, health.Value + heal);
	}

	[ServerRpc]
	public void GiveWeaponServerRpc(NetworkObjectReference weaponRef)
	{
		if (weaponRef.TryGet(out NetworkObject weaponObj))
		{
			weapon = weaponObj.GetComponent<Weapon>();
			// Optionally parent weapon to player, etc.
		}
	}

	[ServerRpc]
	public void RemoveWeaponServerRpc()
	{
		weapon.Value = default;
	}

	public void Update()
    {
		if(!IsOwner) return;	

		if (Input.GetMouseButtonDown(0))
        {
			Debug.Log("shoot");
			
			weapon.Shoot(transform, transform.forward + cam.transform.forward);
        }
    }
}

