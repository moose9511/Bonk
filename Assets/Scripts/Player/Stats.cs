using UnityEngine;
using Unity.Netcode;
using TMPro;

public class Stats : NetworkBehaviour
{
    private float health = 100f;

    public TextMeshProUGUI healthText;


    public void TakeDamage(float damage)
    {
        health -= damage;
        healthText.text = health.ToString("0");
    }

    public void GainHealth(float heal)
    {
        health += heal;
        healthText.text = health.ToString("0");
        if (health > 100f)
        {
            health = 100f;
        }
    }
}
