using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInfo : MonoBehaviour
{
    public WeaponID weaponID;
    public int nAmmo;
    public float range;
    public int damage;

    public float speedOfPlayer;
    // public bool state;
    public float health;

    // [grenade, armor, scope, armor pricing/round]
    public List<bool> equipments;
}
