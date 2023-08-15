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

    public PlayerController player;

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

                CmdDrop(player.weaponInfo);
                Debug.Log("Weapon Dropped!");
                CmdPickUp(playerColliders.OtherCollider.GetComponent<WeaponInfo>());
                Debug.Log("Done!");
            }

            if (Input.GetKeyDown(KeyCode.G))
            {
                player.grenades -= 1;
                CmdThrowGrenade();
            }
        }

    }

    void throwObject(Rigidbody2D rigidBody, Vector2 velocity, float angularVelocity, float drag, float angularDrag)
    {
        rigidBody.velocity = velocity;
        rigidBody.angularVelocity = angularVelocity;
        rigidBody.drag = drag;
        rigidBody.angularDrag = angularDrag;
    }

    [Command]
    void CmdThrowGrenade()
    {
        RpcThrowGrenade();
    }

    [ClientRpc]
    void RpcThrowGrenade()
    {
        GameObject newGrenade = Instantiate(Grenade, transform.position, Quaternion.Euler(0, 0, Random.Range(0, 360)));
        Rigidbody2D weaponRigidbody = newGrenade.GetComponent<Rigidbody2D>();
        throwObject(weaponRigidbody, transform.up * 30f, -50f * 10f, 3.0f, 1f);
    }

    [Command]
    void CmdDrop(WeaponInfo weaponInfo)
    {
        RpcDropWeapon(weaponInfo);
    }

    [ClientRpc]
    void RpcDropWeapon(WeaponInfo weaponInfo)
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
                Instantiate(weapons[weaponInfo.id],
                transform.position,
                Quaternion.Euler(0, 0, Random.Range(0, 360)));
            NetworkServer.Spawn(newWeapon);
            Rigidbody2D weaponRigidbody = newWeapon.GetComponent<Rigidbody2D>();
            // throws object along the ground with a velocity and spin
            throwObject(weaponRigidbody, transform.up * 10f, -50f * 10f, 3.5f, 1f);
            newWeapon.GetComponent<Collider2D>().isTrigger = false;
            StartCoroutine(TurnOnTrigger(newWeapon.GetComponent<Collider2D>()));
            newWeapon.GetComponent<WeaponInfo>().setWeaponInfo(weaponInfo);
        }
    }

    // Refactor: Be able to have a collision order so the weapons collide with walls but not the player
    IEnumerator TurnOnTrigger(Collider2D collider)
    {
        yield return new WaitForSeconds(0.5f); // wait for half a second
        collider.isTrigger = true;
    }

    [Command]
    void CmdPickUp(WeaponInfo weaponInfo)
    {
        RpcDestoryWeapon(weaponInfo);
    }

    [ClientRpc]
    void RpcDestoryWeapon(WeaponInfo weaponInfo)
    {
        if (playerColliders.OtherCollider != null)
        {
            // picks up a weapon on the ground, changes sprite, updates changes player stats, and destroys the weapon
            ChangeSprite(weaponInfo.id);
            AudioSource.PlayClipAtPoint(pickupSound, player.transform.position, AudioListener.volume);
            player.weaponInfo.setWeaponInfo(weaponInfo);
            GetComponent<PlayerController>().cooldownTimer = 0f;
            GetComponent<PlayerController>().isFiring = false;
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
                { Team.Orange, 1 },
                { Team.Red, 2 },
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
