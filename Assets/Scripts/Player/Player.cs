using TMPro;
using UnityEngine;
using Unity.Netcode;

public class Player : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI healthText;

    [SerializeField] private GameObject cam;
    private float health;
    private Weapon weapon;

    public override void OnNetworkSpawn()
    {
        health = 100f;
        weapon = null;
    }

    public void TakeDamage(float damage)
    {
        if (!IsOwner) return;
        health -= damage;
        healthText.text = health.ToString("0");
    }

    public void GainHealth(float heal)
    {
        if (!IsOwner) return;
        health += heal;
        healthText.text = health.ToString("0");
        if (health > 100f)
        {
            health = 100f;
        }
    }

    public void GiveWeapon(Weapon newWeapon)
    {
        if (!IsOwner) return;
        weapon = newWeapon;
    }

    public void RemoveWeapon()
    {
        if (!IsOwner) return;
        weapon = null;
    }

    public void Update()
    {
        if(Input.GetMouseButtonDown(0) && weapon != null)
        {
            weapon.Shoot(transform, transform.forward + cam.transform.forward);
        }
    }
}

