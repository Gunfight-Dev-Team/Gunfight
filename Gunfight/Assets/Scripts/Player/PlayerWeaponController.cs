using System.Collections;
using System.Collections.Generic;
using Mirror;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerWeaponController : MonoBehaviour
{
    [SerializeField] public Team team;

    public PlayerInfo playerInfo;

    //Sprite
    [SerializeField] public Sprite[] greenSprite;
    [SerializeField] public Sprite[] redSprite;
    [SerializeField] public Sprite[] orangeSprite;
    [SerializeField] public Sprite[] whiteSprite;
    public Sprite deadSprite;
    public SpriteRenderer spriteRenderer;

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
        // if (isDead)
        //     spriteRenderer.sprite = deadSprite;
        
        // if (team.Equals(Team.Green))
        // {
        //     // [ ] TODO: change it to be using event intead of update
        //     if (playerInfo.weaponID.Equals(WeaponID.AK47))
        //         spriteRenderer.sprite = greenSprite[0];
        //     else if (playerInfo.weaponID.Equals(WeaponID.Knife))
        //         spriteRenderer.sprite = greenSprite[1];
        //     else if (playerInfo.weaponID.Equals(WeaponID.Pistol))
        //         spriteRenderer.sprite = greenSprite[2];
        //     else if (playerInfo.weaponID.Equals(WeaponID.Sniper))
        //         spriteRenderer.sprite = greenSprite[3];
        //     else if (playerInfo.weaponID.Equals(WeaponID.Uzi))
        //         spriteRenderer.sprite = greenSprite[4];
        // }

        // if (team.Equals(Team.Red))
        // {
        //     // [ ] TODO: change it to be using event intead of update
        //     if (playerInfo.weaponID.Equals(WeaponID.AK47))
        //         spriteRenderer.sprite = redSprite[0];
        //     else if (playerInfo.weaponID.Equals(WeaponID.Knife))
        //         spriteRenderer.sprite = redSprite[1];
        //     else if (playerInfo.weaponID.Equals(WeaponID.Pistol))
        //         spriteRenderer.sprite = redSprite[2];
        //     else if (playerInfo.weaponID.Equals(WeaponID.Sniper))
        //         spriteRenderer.sprite = redSprite[3];
        //     else if (playerInfo.weaponID.Equals(WeaponID.Uzi))
        //         spriteRenderer.sprite = redSprite[4];
        // }

        // if (team.Equals(Team.Orange))
        // {
        //     // [ ] TODO: change it to be using event intead of update
        //     if (playerInfo.weaponID.Equals(WeaponID.AK47))
        //         spriteRenderer.sprite = orangeSprite[0];
        //     else if (playerInfo.weaponID.Equals(WeaponID.Knife))
        //         spriteRenderer.sprite = orangeSprite[1];
        //     else if (playerInfo.weaponID.Equals(WeaponID.Pistol))
        //         spriteRenderer.sprite = orangeSprite[2];
        //     else if (playerInfo.weaponID.Equals(WeaponID.Sniper))
        //         spriteRenderer.sprite = orangeSprite[3];
        //     else if (playerInfo.weaponID.Equals(WeaponID.Uzi))
        //         spriteRenderer.sprite = orangeSprite[4];
        // }

        // if (team.Equals(Team.White))
        // {
        //     // [ ] TODO: change it to be using event intead of update
        //     if (playerInfo.weaponID.Equals(WeaponID.AK47))
        //         spriteRenderer.sprite = whiteSprite[0];
        //     else if (playerInfo.weaponID.Equals(WeaponID.Knife))
        //         spriteRenderer.sprite = whiteSprite[1];
        //     else if (playerInfo.weaponID.Equals(WeaponID.Pistol))
        //         spriteRenderer.sprite = whiteSprite[2];
        //     else if (playerInfo.weaponID.Equals(WeaponID.Sniper))
        //         spriteRenderer.sprite = whiteSprite[3];
        //     else if (playerInfo.weaponID.Equals(WeaponID.Uzi))
        //         spriteRenderer.sprite = whiteSprite[4];
        // }
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
    }
}
