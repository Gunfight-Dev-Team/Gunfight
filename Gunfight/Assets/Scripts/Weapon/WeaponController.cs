using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// If time permit, take a look at Object Pool
public class WeaponController : MonoBehaviour
{
    [SerializeField] private GameObject AK47;
    [SerializeField] private GameObject Knife;
    [SerializeField] private GameObject Pistol;
    [SerializeField] private GameObject Sniper;
    [SerializeField] private GameObject Uzi;

    public WeaponInfo weapon;
    public GameObject Player;

    private bool canPickup = false;

    private Collider2D OtherCollider;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            canPickup = true;
            OtherCollider = other;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            canPickup = false;
        }
    }

    void Update()
    {
        if (canPickup && Input.GetKeyDown(KeyCode.E))
        {
            // Pick up the weapon
            Debug.Log("Weapon picked up!");
            Drop(OtherCollider.GetComponent<PlayerInfo>().weaponID, OtherCollider);
            PickUp(OtherCollider);
        }
    }

    void Drop(WeaponID newWeaponID, Collider2D other)
    {
        // [ ] TODO: is it possible to make this more simple?
        var weapons = new Dictionary<WeaponID, GameObject>(){
            {WeaponID.AK47, AK47},
            {WeaponID.Knife, Knife},
            {WeaponID.Pistol, Pistol},
            {WeaponID.Sniper, Sniper},
            {WeaponID.Uzi, Uzi}
        };

        GameObject newWeapon = Instantiate(weapons[newWeaponID], 
                                    OtherCollider.GetComponent<Transform>().position, 
                                    weapons[newWeaponID].transform.rotation);
        newWeapon.GetComponent<WeaponInfo>().nAmmo = other.GetComponent<PlayerInfo>().nAmmo;
        newWeapon.GetComponent<WeaponInfo>().range = other.GetComponent<PlayerInfo>().range;
        newWeapon.GetComponent<WeaponInfo>().speedOfPlayer = other.GetComponent<PlayerInfo>().speedOfPlayer;
    }

    void PickUp(Collider2D other)
    {
        other.GetComponent<PlayerInfo>().weaponID = weapon.id;
        other.GetComponent<PlayerInfo>().nAmmo = weapon.nAmmo;
        other.GetComponent<PlayerInfo>().range = weapon.range;
        other.GetComponent<PlayerInfo>().speedOfPlayer = weapon.speedOfPlayer;
        Destroy(gameObject);
    }
}
