using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class WeaponDataBase : NetworkBehaviour
{
    [SerializeField] private Weapon[] weapons;
    public static Dictionary<int, Weapon> Weapons = new Dictionary<int, Weapon>();

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        for(int i = 0; i < weapons.Length; i++)
        {
            weapons[i].weaponId = i;
            Weapons.Add(i, weapons[i]);
        }
    }

    public static Weapon GetWeaponById(int id)
    {
        return Weapons.ContainsKey(id) ? Weapons[id] : null;
    }

    public static Weapon GetRandomWeapon()
    {
        int randomId = Random.Range(0, Weapons.Count);
        return Weapons[randomId];
    }
}
