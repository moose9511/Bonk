using TMPro;
using UnityEngine;
using Unity.Netcode;

public class Player : NetworkBehaviour
{
	// health text
	[SerializeField] private GameObject canvas;

	[SerializeField] private TextMeshProUGUI healthText;

    [SerializeField] private GameObject cam;
	[SerializeField] private GameObject gun;

	// health
    private NetworkVariable<float> health = new(
		100f, NetworkVariableReadPermission.Owner, NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
		// sets onvaluechanged methods to network variables
		Debug.Log("spawn");
		gun.SetActive(false);

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
		gameObject.SetActive(true);
	}

	public void GiveWeapon()
	{
		if (!IsOwner) return;

		Debug.Log("what");
		gun.SetActive(true);
	}

	[ServerRpc]
	public void RemoveWeaponServerRpc()
	{
	}

	public void Update()
    {
		if(!IsOwner) return;	

		if (Input.GetMouseButtonDown(0))
        {
			Debug.Log("shoot");
			
        }
    }
}

