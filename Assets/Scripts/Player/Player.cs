using TMPro;
using UnityEngine;
using Unity.Netcode;

public class Player : NetworkBehaviour
{
	// health text
	[SerializeField] private GameObject canvas;

	[SerializeField] private TextMeshProUGUI healthText;

    [SerializeField] private GameObject cam;

	public static int groundLayer;

	// health
    private NetworkVariable<float> health = new(
		100f, NetworkVariableReadPermission.Owner, NetworkVariableWritePermission.Server);

	private Weapon weapon;

    public override void OnNetworkSpawn()
    {

        groundLayer = LayerMask.NameToLayer("Ground");

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
		Debug.Log("take damage");
	}

	[ServerRpc]
	public void GainHealthServerRpc(float heal)
	{
		health.Value = Mathf.Min(100f, health.Value + heal);
	}

	[ServerRpc]
	public void GiveWeaponServerRpc()
	{
		
	}

	public void GiveWeapon(GameObject weapon)
	{
		this.weapon = weapon.GetComponent<Weapon>();
    }

	[ServerRpc]
	public void RemoveWeaponServerRpc()
	{
	}

	public void Update()
    {
		if(!IsOwner) return;	

		if (weapon != null && Input.GetMouseButtonDown(0))
        {
			weapon.ShootServerRpc();
        }
    }
}

