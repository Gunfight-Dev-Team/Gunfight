using System.Collections;
using System.Collections.Generic;
using Mirror;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerWeaponController : MonoBehaviour
{
    [SerializeField] internal Team team;

    public PlayerInfo playerInfo;

    //Sprite
    public SpriteRenderer spriteRenderer;
    [SerializeField] internal List<Sprite> spriteArray;

    //Weapon
    private bool canPickup = false;
    private Collider2D OtherCollider;
    [SerializeField] private GameObject AK47;
    [SerializeField] private GameObject Knife;
    [SerializeField] private GameObject Pistol;
    [SerializeField] private GameObject Sniper;
    [SerializeField] private GameObject Uzi;

    private void Update()
    {
        if (SceneManager.GetActiveScene().name == "Game")
        {
            if (canPickup && Input.GetKeyDown(KeyCode.E))
                {
                    // Pick up the weapon
                    Debug.Log("Weapon picked up!");
                    Drop();
                    PickUp();
                }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Weapon"))
        {
            canPickup = true;
            OtherCollider = other;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Weapon"))
        {
            canPickup = false;
        }
    }

    void Drop()
    {
        // [ ] TODO: is it possible to make this more simple?
        var weapons = new Dictionary<WeaponID, GameObject>(){
            {WeaponID.AK47, AK47},
            {WeaponID.Knife, Knife},
            {WeaponID.Pistol, Pistol},
            {WeaponID.Sniper, Sniper},
            {WeaponID.Uzi, Uzi}
        };

        WeaponID newWeaponID = playerInfo.weaponID;
        GameObject newWeapon = Instantiate(weapons[newWeaponID], 
                                    this.transform.position, 
                                    weapons[newWeaponID].transform.rotation);
        newWeapon.GetComponent<WeaponInfo>().nAmmo = playerInfo.nAmmo;
        newWeapon.GetComponent<WeaponInfo>().range = playerInfo.range;
        newWeapon.GetComponent<WeaponInfo>().speedOfPlayer = playerInfo.speedOfPlayer;
    }

    void PickUp()
    {
        playerInfo.weaponID = OtherCollider.GetComponent<WeaponInfo>().id;
        playerInfo.nAmmo = OtherCollider.GetComponent<WeaponInfo>().nAmmo;
        playerInfo.speedOfPlayer = OtherCollider.GetComponent<WeaponInfo>().speedOfPlayer;
        Destroy(OtherCollider.gameObject);
        ChangeSprite(playerInfo.weaponID);
    }

    void ChangeSprite(WeaponID weapon)
    {
        // [ ] TODO: is it possible to make this more simple?
        var weaponArray = new Dictionary<WeaponID, int>(){
            {WeaponID.AK47, 0},
            {WeaponID.Knife, 1},
            {WeaponID.Pistol, 2},
            {WeaponID.Sniper, 3},
            {WeaponID.Uzi, 4}
        };
        
        // [ ] TODO: is it possible to make this more simple?
        var teamArray = new Dictionary<Team, int>(){
            {Team.Green, 0},
            {Team.Red, 1},
            {Team.Orange, 2},
            {Team.White, 3}
        };

        // change sprite
        int index = weaponArray[weapon]*4 + teamArray[team];
        spriteRenderer.sprite = spriteArray[index];
    }
}
