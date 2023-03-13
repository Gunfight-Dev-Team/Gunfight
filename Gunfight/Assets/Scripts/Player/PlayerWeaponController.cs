using System.Collections;
using System.Collections.Generic;
using Mirror;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerWeaponController : NetworkBehaviour
{
    [SerializeField] internal Team team;

    public PlayerInfo playerInfo;
    public GameObject playerRef;

    //Sprite
    public SpriteRenderer spriteRenderer;
    public Sprite deadSprite;
    [SerializeField] internal List<Sprite> spriteArray;

    //Weapon
    public PlayerColliders playerColliders;
    [SerializeField] private GameObject AK47;
    [SerializeField] private GameObject Knife;
    [SerializeField] private GameObject Pistol;
    [SerializeField] private GameObject Sniper;
    [SerializeField] private GameObject Uzi;

    [ClientCallback]
    private void Update()
    {
        if(!isOwned) return;

        if (SceneManager.GetActiveScene().name == "Game")
        {
            if (playerColliders.canPickup && Input.GetKeyDown(KeyCode.E))
            {
                    // Pick up the weapon
                Debug.Log("Weapon picked up!");
                if (isServer)
                {
                    CmdDrop(playerInfo.weaponID, playerInfo.nAmmo, playerInfo.range, playerInfo.speedOfPlayer);
                    CmdPickUp(playerColliders.OtherCollider.GetComponent<WeaponInfo>().id, playerColliders.OtherCollider.GetComponent<WeaponInfo>().nAmmo, playerColliders.OtherCollider.GetComponent<WeaponInfo>().speedOfPlayer);
                }
            }
        }
    }

    [Command]
    void CmdDrop(WeaponID newWeaponID, int nAmmo, float range, float speedOfPlayer)
    {
        RpcDropWeapon();
    }

    [ClientRpc]
    void RpcDropWeapon()
    {
        // [ ] TODO: is it possible to make this more simple?
        var weapons = new Dictionary<WeaponID, GameObject>(){
            {WeaponID.AK47, AK47},
            {WeaponID.Knife, Knife},
            {WeaponID.Pistol, Pistol},
            {WeaponID.Sniper, Sniper},
            {WeaponID.Uzi, Uzi}
        };

        GameObject newWeapon = Instantiate(weapons[playerInfo.weaponID],
                                    playerRef.transform.position,
                                    weapons[playerInfo.weaponID].transform.rotation);
        newWeapon.GetComponent<WeaponInfo>().nAmmo = playerInfo.nAmmo;
        newWeapon.GetComponent<WeaponInfo>().range = playerInfo.range;
        newWeapon.GetComponent<WeaponInfo>().speedOfPlayer = playerInfo.speedOfPlayer;

        NetworkServer.Spawn(newWeapon);
    }

        [Command]
    void CmdPickUp(WeaponID weapon, int nAmmo, float speedOfPlayer)
    { 
        RpcDestoryWeapon(weapon, nAmmo, speedOfPlayer);
    }

    [ClientRpc]
    void RpcDestoryWeapon(WeaponID weapon, int nAmmo, float speedOfPlayer)
    {
        ChangeSprite(weapon);
        playerInfo.weaponID = weapon;
        playerInfo.nAmmo = nAmmo;
        playerInfo.speedOfPlayer = speedOfPlayer;
        Destroy(playerColliders.OtherCollider.gameObject);
        NetworkServer.Destroy(playerColliders.OtherCollider.gameObject);
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

        if (weapon == (WeaponID)(-1))
        {
            spriteRenderer.sprite = deadSprite;
        }
        else
        {
            // change sprite
            int index = weaponArray[weapon] * 4 + teamArray[team];
            spriteRenderer.sprite = spriteArray[index];
        }
    }
}
