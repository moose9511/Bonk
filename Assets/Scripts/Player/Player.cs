using System.Collections;
using System.Runtime.CompilerServices;
using Newtonsoft.Json.Linq;
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
	[SerializeField] private TextMeshProUGUI ammoCount;
	[SerializeField] private TextMeshProUGUI powerText;
	[SerializeField] private Button respawnBtn;
	[SerializeField] private GameObject gunPointer;

    [SerializeField] private GameObject cam;

	[SerializeField] private LayerMask pickupMask;
	[SerializeField] private LayerMask boundsMask;

	private Coroutine jumpPowerRoutine;
	private Coroutine speedPowerRoutine;
	private Coroutine hardPowerRoutine;

	private Transform deadTransform;

	public static int groundLayer;

	private float shootInterval = 1, shootTime;
	public int Ammo
	{
		get { return ammo; }
		set
		{
			ammo = value;
			ammoCount.text = ammo.ToString();
		}
	}
	private int ammo;

	public Vector3 respawnPos;
	public Vector3 deadPos;

	[SerializeField] private MeshFilter filter;

	// health
    private NetworkVariable<float> health = new(
		100f, NetworkVariableReadPermission.Owner, NetworkVariableWritePermission.Server);
	private NetworkVariable<int> weaponId = new(
		-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

	public Weapon currentWeapon = null;

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
			else if (SceneManager.GetActiveScene().name == "map1")
			{
				Debug.Log("Starting animations for player client");	
				canvas.SetActive(true);
			}
				
		}

    }

	[ClientRpc]
	public void StartAnimationsClientRpc()
	{
		Debug.Log("StartAnimationsClientRpc called");
		StartCoroutine(AnimationManager.StartAnimations());

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
		if(health.Value > 100f) return;
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

		if(health.Value <= 0)
		{
			BeDead();
		} 

		// checks to see if player has collided with a pickup
		Collider[] pickups = Physics.OverlapCapsule(transform.position + new Vector3(0, .5f, 0), transform.position - new Vector3(0, .5f, 0), .6f, pickupMask);
		foreach (Collider coll in pickups)
		{
			Pickup pickup = coll.gameObject.GetComponent<Pickup>();

			if(pickup.isWeapon)
			{
                currentWeapon = pickup.weapon;
                currentWeapon.clientOwnerId = (int)OwnerClientId;
                Ammo = pickup.weapon.ammo;

                shootInterval = currentWeapon.fireRate;
                shootTime = shootInterval;
            } else if (pickup.isHealth)
			{
				GainHealthServerRpc(Mathf.Clamp(pickup.healthGained, 0, 100));
			} else if (pickup.isPower)
			{
				int randPow = Random.Range(1, 5);

				Debug.Log("Powerup: " + randPow);

				switch (randPow)
				{
					case 1:
                        StopAllPowers(speed: false, hard: false);
                        jumpPowerRoutine = StartCoroutine(JumpPower(15));
						break;
					case 2:
						StopAllPowers(jump: false, hard: false);
                        speedPowerRoutine = StartCoroutine(SpeedPower(8));
						break;
					case 3:
                        StopAllPowers(speed: false, jump: false);
                        hardPowerRoutine = StartCoroutine(HardPower(50));
						break;
					case 4:
						ExtraHealthPowerServerRpc(200);
						break;
				}
			}

			pickup.DieServerRpc();
			Destroy(pickup.gameObject);
		}

        Collider[] outOfBounds = Physics.OverlapCapsule(transform.position + new Vector3(0, .5f, 0), transform.position - new Vector3(0, .5f, 0), .6f, boundsMask);
		if(outOfBounds.Length > 0)
		{
			BeDead();
		}

        if (currentWeapon == null) 
		{
			filter.mesh = null;
			Ammo = 0;

			if(Input.GetMouseButtonDown(0) && shootTime >= shootInterval)
			{
				shootTime = 0;
                Physics.SphereCast(cam.transform.position, .4f, cam.transform.forward, out RaycastHit punchHit, 3.3f);
                if (punchHit.collider != null && punchHit.collider.CompareTag("Player"))
                {
					if (punchHit.collider.gameObject.GetComponent<NetworkObject>().OwnerClientId == OwnerClientId) return;

                    punchHit.collider.gameObject.GetComponent<Player>().TakeDamageServerRpc(2);
                    punchHit.collider.gameObject.GetComponent<PlayerMovement2>().AddForceRpc(cam.transform.forward * 20);
                }
            }
            if (shootTime < shootInterval)
                shootTime += Time.deltaTime;

            return;
		} else
		{
			filter.mesh = currentWeapon.weaponMesh;
		}

		if (shootTime >= shootInterval)
		{
			bool shoot = false;
			if(currentWeapon.isAutomatic)
				shoot = Input.GetMouseButton(0);
			else
				shoot = Input.GetMouseButtonDown(0);

			if (shoot)
			{
				float[] values = currentWeapon.SerializeData(cam.transform.forward);
				SpawnProjServerRpc(values, currentWeapon.weaponId);
				shootTime = 0;

				Ammo -= 1;
				if (Ammo <= 0)
				{
					currentWeapon = null;
				}
			}
		}
		else
		{
			if(shootTime < shootInterval)
				shootTime += Time.deltaTime;
		}
				
    }

	public IEnumerator JumpPower(float jumpPower)
	{
		GetComponent<PlayerMovement2>().jumpSpeed += jumpPower;
		for(int i = 40; i > 0; i--)
		{
			powerText.text = "+Jump :" + i;
            yield return new WaitForSeconds(1);
        }

		powerText.text = "";
		GetComponent<PlayerMovement2>().jumpSpeed -= jumpPower;
	}
    public IEnumerator SpeedPower(float speedPower)
    {
        GetComponent<PlayerMovement2>().moveSpeed += speedPower;
        for (int i = 25; i > 0; i--)
        {
            powerText.text = "+Speed :" + i;
            yield return new WaitForSeconds(1);
        }
        powerText.text = "";
        GetComponent<PlayerMovement2>().moveSpeed -= speedPower;
    }

	[Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void ExtraHealthPowerServerRpc(float extra)
    {
		health.Value = extra;
    }
    public IEnumerator HardPower(float threshold)
    {
        GetComponent<PlayerMovement2>().hitThreshold += threshold;
        for (int i = 45; i > 0; i--)
        {
            powerText.text = "+Durability :" + i;
            yield return new WaitForSeconds(1);
        }
        powerText.text = "";
        GetComponent<PlayerMovement2>().hitThreshold -= threshold;
    }

    public IEnumerator RespawnCountDown()
    {
		respawnBtn.gameObject.SetActive(true);
		respawnBtn.enabled = true;
        respawnBtn.interactable = false;

        var text = respawnBtn.GetComponentInChildren<TextMeshProUGUI>();

        for (int i = 3; i > 0; i--)
        {
            text.text = i.ToString();
            yield return new WaitForSecondsRealtime(1f);
        }

        respawnBtn.interactable = true;

        text.text = "RESPAWN";
    }

    public void BeDead()
	{
        var camMovement = GetComponent<CameraMovement>();
        camMovement.useSceneCam = true;
        camMovement.UseCorrectCameras();

		GetComponent<PlayerMovement2>().extraForce = Vector3.zero;

		ammoCount.enabled = false;
		healthText.enabled = false;
		powerText.enabled = false;
		respawnBtn.enabled = true;
		currentWeapon = null;
		gunPointer.SetActive(false);

		StopAllPowers();

		ammo = -1;

		transform.position = deadPos;

        StartCoroutine(RespawnCountDown());

    }

	public void Respawn()
	{
        GainHealthServerRpc(100);
		ammoCount.enabled = true;
		healthText.enabled = true;
		respawnBtn.gameObject.SetActive(false);
        powerText.enabled = true;
		gunPointer.SetActive(true);

        GetComponent<PlayerMovement2>().extraForce = Vector3.zero;

		transform.position = respawnPos;

		respawnBtn.enabled = false;

        var playercam = GetComponent<CameraMovement>();
        playercam.useSceneCam = false;
        playercam.UseCorrectCameras();
    }


    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
	public void SpawnProjServerRpc(float[] values, int weaponId)
	{
		Weapon shotWeaon = WeaponDataBase.GetWeaponById(weaponId);

        var projObj = Instantiate(shotWeaon.projPrefab,
        cam.transform.position + cam.transform.forward * shotWeaon.distanceToShooter,
        Quaternion.identity);

		var network = projObj.GetComponent<NetworkObject>();
		network.Spawn();
        SpawnVisualProjClientRpc(weaponId, cam.transform.position, cam.transform.forward, network.NetworkObjectId);
        var proj = projObj.GetComponent<Projectile>();

		proj.Init(values);
	}

	[ClientRpc]
	public void SpawnVisualProjClientRpc(int weaponId, Vector3 position, Vector3 lookDir, ulong networkId)
	{
        Weapon shotWeaon = WeaponDataBase.GetWeaponById(weaponId);
		var projObj = Instantiate(shotWeaon.projPrefab,
		position + lookDir * shotWeaon.distanceToShooter,
		Quaternion.identity);

        projObj.GetComponent<Renderer>().enabled = true;

        projObj.transform.localScale = new Vector3(shotWeaon.radius * 2, shotWeaon.radius * 2, shotWeaon.radius * 2);

        projObj.GetComponent<Projectile>().Init(shotWeaon.speed, position, lookDir, networkId);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
	public void SetWeaponServerRpc(int weaponId)
	{
		currentWeapon = WeaponDataBase.GetWeaponById(weaponId);
	}

	public void StopAllPowers(bool jump = true, bool speed = true, bool hard = true)
	{
		powerText.text = "";
        if(jumpPowerRoutine != null && jump)
		{
            StopCoroutine(jumpPowerRoutine);
			GetComponent<PlayerMovement2>().jumpSpeed = 6f;
        }
        if (speedPowerRoutine != null && speed)
        {
            StopCoroutine(speedPowerRoutine);
            GetComponent<PlayerMovement2>().moveSpeed = 10f;
        }
        if (hardPowerRoutine != null && hard)
        {
            StopCoroutine(hardPowerRoutine);
			GetComponent<PlayerMovement2>().hitThreshold = 4;
        }
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

