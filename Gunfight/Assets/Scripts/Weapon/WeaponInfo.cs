using UnityEngine;

public enum WeaponID
{
    AK47,
    Knife,
    Pistol,
    Sniper,
    Uzi
}

public class WeaponInfo : MonoBehaviour
{
    public WeaponID id;

    public int nAmmo;

    public int damage;

    public float range;

    public float speedOfPlayer;

    public float cooldown;

    public bool isAuto;

    public bool isMelee;
}
