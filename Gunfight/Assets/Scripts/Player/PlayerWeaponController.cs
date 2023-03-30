using System.Collections;
using System.Collections.Generic;
using Mirror;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class PlayerWeaponController : NetworkBehaviour
{
    [SerializeField]
    internal Team team;

    public PlayerInfo playerInfo;

    public GameObject playerRef;

    //Sprite
    public SpriteRenderer spriteRenderer;

    [SerializeField]
    internal List<Sprite> spriteArray;

    //Weapon
    public PlayerColliders playerColliders;

    public AudioClip pickupSound;

    [SerializeField]
    private GameObject AK47;

    [SerializeField]
    private GameObject Knife;

    [SerializeField]
    private GameObject Pistol;

    [SerializeField]
    private GameObject Sniper;

    [SerializeField]
    private GameObject Uzi;

    [SerializeField]
    private GameObject Grenade;

    private void Update()
    {
        if (!isLocalPlayer) return;

        if (SceneManager.GetActiveScene().name == "Game")
        {
            if (playerColliders.canPickup && Input.GetKeyDown(KeyCode.Mouse1))
            {
                // Pick up the weapon
                Debug.Log("Weapon picked up!");

                CmdDrop(playerInfo.weaponID,
                playerInfo.nAmmo,
                playerInfo.range,
                playerInfo.damage,
                playerInfo.speedOfPlayer);
                CmdPickUp(playerColliders
                    .OtherCollider
                    .GetComponent<WeaponInfo>()
                    .id,
                playerColliders.OtherCollider.GetComponent<WeaponInfo>().nAmmo,
                playerColliders.OtherCollider.GetComponent<WeaponInfo>().range,
                playerColliders.OtherCollider.GetComponent<WeaponInfo>().damage,
                playerColliders
                    .OtherCollider
                    .GetComponent<WeaponInfo>()
                    .cooldown,
                playerColliders
                    .OtherCollider
                    .GetComponent<WeaponInfo>()
                    .speedOfPlayer);
            }

            if(Input.GetKeyDown(KeyCode.G))
            {
                playerInfo.grenades -= 1;
                CmdThrowGrenade();
            }
        }

    }

    [Command]
    void CmdThrowGrenade()
    {
        RpcThrowGrenade();
    }

    [ClientRpc]
    void RpcThrowGrenade()
    {
        GameObject newGrenade =
                Instantiate(Grenade,
                playerRef.transform.position,
                Quaternion.Euler(0, 0, Random.Range(0, 360)));
        Rigidbody2D weaponRigidbody = newGrenade.GetComponent<Rigidbody2D>();
        weaponRigidbody.velocity = playerRef.transform.up * 30f;
        weaponRigidbody.angularVelocity = -50f * 10f;
        weaponRigidbody.drag = 3.0f;
        weaponRigidbody.angularDrag = 1f;
    }

    [Command]
    void CmdDrop(
        WeaponID newWeaponID,
        int nAmmo,
        float range,
        int damage,
        float speedOfPlayer
    )
    {
        RpcDropWeapon();
    }

    [ClientRpc]
    void RpcDropWeapon()
    {
        if (playerColliders.OtherCollider != null)
        {
            // [ ] TODO: is it possible to make this more simple?
            var weapons =
                new Dictionary<WeaponID, GameObject>()
                {
                { WeaponID.AK47, AK47 },
                { WeaponID.Knife, Knife },
                { WeaponID.Pistol, Pistol },
                { WeaponID.Sniper, Sniper },
                { WeaponID.Uzi, Uzi }
                };

            GameObject newWeapon =
                Instantiate(weapons[playerInfo.weaponID],
                playerRef.transform.position,
                Quaternion.Euler(0, 0, Random.Range(0, 360)));
            Rigidbody2D weaponRigidbody = newWeapon.GetComponent<Rigidbody2D>();
            weaponRigidbody.velocity = playerRef.transform.up * 10f;
            weaponRigidbody.angularVelocity = -50f * 10f;
            weaponRigidbody.drag = 3.5f;
            weaponRigidbody.angularDrag = 1f;
            newWeapon.GetComponent<Collider2D>().isTrigger = false;
            StartCoroutine(TurnOnTrigger(newWeapon.GetComponent<Collider2D>()));
            newWeapon.GetComponent<WeaponInfo>().nAmmo = playerInfo.nAmmo;
            newWeapon.GetComponent<WeaponInfo>().range = playerInfo.range;
            newWeapon.GetComponent<WeaponInfo>().damage = playerInfo.damage;
            newWeapon.GetComponent<WeaponInfo>().speedOfPlayer =
                playerInfo.speedOfPlayer;
        }
        //if(isServer)
        //NetworkServer.Spawn(newWeapon);
    }

    IEnumerator TurnOnTrigger(Collider2D collider)
    {
        yield return new WaitForSeconds(0.5f); // wait for half a second
        collider.isTrigger = true;
    }

    [Command]
    void CmdPickUp(
        WeaponID weapon,
        int nAmmo,
        float range,
        int damage,
        float cooldown,
        float speedOfPlayer
    )
    {
        RpcDestoryWeapon (
            weapon,
            nAmmo,
            range,
            damage,
            cooldown,
            speedOfPlayer
        );
    }

    [ClientRpc]
    void RpcDestoryWeapon(
        WeaponID weapon,
        int nAmmo,
        float range,
        int damage,
        float cooldown,
        float speedOfPlayer
    )
    {
        if (playerColliders.OtherCollider != null)
        {
            ChangeSprite(weapon);
            AudioSource
                    .PlayClipAtPoint(pickupSound, playerInfo.transform.position, AudioListener.volume);
            playerInfo.weaponID = weapon;
            playerInfo.nAmmo = nAmmo;
            playerInfo.range = range;
            playerInfo.damage = damage;
            playerInfo.cooldown = cooldown;
            GetComponent<PlayerMovementController>().cooldownTimer = 0f;
            GetComponent<PlayerMovementController>().isFiring = false;
            if (weapon == WeaponID.AK47 || weapon == WeaponID.Uzi)
                playerInfo.isAuto = true;
            else
                playerInfo.isAuto = false;
            if (weapon == WeaponID.Knife)
                playerInfo.isMelee = true;
            else
                playerInfo.isMelee = false;
            playerInfo.speedOfPlayer = speedOfPlayer;
            Destroy(playerColliders.OtherCollider.gameObject);
            if (isServer)
                NetworkServer.Destroy(playerColliders.OtherCollider.gameObject);
            playerColliders.OtherCollider = null;
        }
    }

    void ChangeSprite(WeaponID weapon)
    {
        // [ ] TODO: is it possible to make this more simple?
        var weaponArray =
            new Dictionary<WeaponID, int>()
            {
                { WeaponID.AK47, 0 },
                { WeaponID.Knife, 1 },
                { WeaponID.Pistol, 2 },
                { WeaponID.Sniper, 3 },
                { WeaponID.Uzi, 4 }
            };

        // [ ] TODO: is it possible to make this more simple?
        var teamArray =
            new Dictionary<Team, int>()
            {
                { Team.Green, 0 },
                { Team.Red, 1 },
                { Team.Orange, 2 },
                { Team.White, 3 }
            };

        if (weapon == (WeaponID)(-1))
        {
            int index = 5 * 4 + teamArray[team];
            spriteRenderer.sprite = spriteArray[index];
        }
        else
        {
            // change sprite
            int index = weaponArray[weapon] * 4 + teamArray[team];
            spriteRenderer.sprite = spriteArray[index];
        }
    }
}
