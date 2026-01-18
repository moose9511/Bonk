using System.Runtime.CompilerServices;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Player : NetworkBehaviour
{
	// health text
	[SerializeField] public GameObject canvas;

	[SerializeField] private TextMeshProUGUI healthText;

    [SerializeField] private GameObject cam;

	[SerializeField] private LayerMask pickupMask;

	public static int groundLayer;

	private float shootInterval, shootTime;

	public Vector3 respawnPos;

	[SerializeField] private MeshFilter filter;

	// health
    private NetworkVariable<float> health = new(
		100f, NetworkVariableReadPermission.Owner, NetworkVariableWritePermission.Server);
	private NetworkVariable<int> weaponId = new(
		-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

	public Weapon currentWeapon = null;
	public bool dead = false;

    public override void OnNetworkSpawn()
    {
		
		base.OnNetworkSpawn();
        // sets ground layer
        groundLayer = LayerMask.NameToLayer("Ground");

		// sets correct canvas for each player
		if (!IsOwner) { 
			canvas.SetActive(false);
		}
		else
		{
			DontDestroyOnLoad(gameObject);
			health.OnValueChanged += OnHealthChanged;
			if (SceneManager.GetActiveScene().name == "WaitingRoom")
			{
				canvas.SetActive(false);
			}
			else
			{
				canvas.SetActive(true);
			}
				
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
	
	[Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
	public void DIEFOOLServerRpc(ulong clientId)
	{
		if(OwnerClientId == clientId)
		{
			transform.position = new Vector3(-99999999, -99999999, -99999999);
        }
			
	}

    public void Update()
    {
		if(!IsOwner) return;

        if (dead && health.Value > 0)
		{
			SetPosRpc(respawnPos.x, respawnPos.y, respawnPos.z);
			dead = false;
		} else if (dead)
		{
            SetPosRpc(-999999, 99999, -999999);
            GetComponent<PlayerMovement2>().extraForce = Vector3.zero;
            return;
        }

		if(health.Value <= 0)
		{
			BeDead();
		}

		// checks to see if player has collided with a pickup
		Collider[] pickups = Physics.OverlapCapsule(transform.position + new Vector3(0, .5f, 0), transform.position - new Vector3(0, .5f, 0), .6f, pickupMask);
		foreach (Collider coll in pickups)
		{
			Pickup pickup = coll.gameObject.GetComponent<Pickup>();

			currentWeapon = pickup.weapon;
			currentWeapon.clientOwnerId = (int)OwnerClientId;

			shootInterval = currentWeapon.fireRate;
			shootTime = shootInterval;

            pickup.DieServerRpc();
		}


		if (currentWeapon == null) 
		{
			filter.mesh = null;
			return;
		} else
		{
				filter.mesh = currentWeapon.weaponMesh;

		}

		if (shootTime >= shootInterval)
		{
			if (Input.GetMouseButtonDown(0))
			{
				float[] values = currentWeapon.SerializeData(cam.transform.forward);
				SpawnProjServerRpc(values, currentWeapon.weaponId);
				shootTime = 0;
			}
		}
		else
		{
			shootTime += Time.deltaTime;
		}
				
    }

	public void BeDead()
	{
        var playerSpawner = FindAnyObjectByType<SpawnPlayer>();

        var camMovement = GetComponent<CameraMovement>();
        camMovement.useSceneCam = true;
        camMovement.UseCorrectCameras();

        canvas.SetActive(false);

        StartCoroutine(playerSpawner.RespawnCountDown(gameObject, FindAnyObjectByType<CustomPlayerSpawner>().transform));

        SetPosRpc(-999999, 99999, -999999);
        dead = true;
    }

	[Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
	public void SetPosRpc(float x, float y, float z)
	{
		transform.position = new Vector3(x, y, z);	
	}

	[Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
	public void SpawnProjServerRpc(float[] values, int weaponId)
	{
		Weapon shotWeaon = WeaponDataBase.GetWeaponById(weaponId);
        var projObj = Instantiate(shotWeaon.projPrefab,
        cam.transform.position + cam.transform.forward * shotWeaon.distanceToShooter,
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

	public void SetState(string state)
	{
		if (!IsOwner) return;

		if (state.Equals("map1"))
		{
			canvas.SetActive(true);
		} else if (state.Equals("WaitingRoom"))
		{
			canvas.SetActive(false);
		}
	}

}

