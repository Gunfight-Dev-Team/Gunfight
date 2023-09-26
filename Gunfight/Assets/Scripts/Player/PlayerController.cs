using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Mirror;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.U2D.Animation;
using UnityEngine.UI;


public enum Team
{
    Green,
    Orange,
    Red,
    White
}

public class PlayerController : NetworkBehaviour
{
    public WeaponInfo weaponInfo;
    public GameObject weapon;
    public int grenades;

    public bool hasSpawned = false;

    public Rigidbody2D rb;

    public Camera cam;

    public PlayerObjectController poc;

    [SerializeField] 
    public Team team;

    //Sprite

    public SpriteRenderer spriteRenderer;
    public SpriteRenderer weaponSpriteRenderer;

    public Animator playerAnimator;

    public SpriteLibraryAsset[] spriteLibraryArray;

    //Shooting
    public Transform shootPoint;

    public float cooldownTimer = 0;

    public bool isFiring;

    public CameraShaker CameraShaker;

    [SyncVar]
    public float health = 10f;

    public bool alive = true;

    public GameObject hitParticle;

    public GameObject bulletParticle;

    public AudioClip PistolShotSound;

    public AudioClip UziShotSound;

    public AudioClip SniperShotSound;

    public AudioClip AK47ShotSound;

    public AudioClip KnifeSound;

    public AudioClip Walk_1;

    public AudioClip Walk_2;

    public AudioClip Walk_3;

    public AudioClip Walk_4;

    public AudioClip emptySound;

    public AudioClip breakSound;

    public AudioClip[] HurtsSound;

    private AudioSource audioSource;

    [SerializeField] private GameObject ammo;

    public SpriteLibrary spriteLibrary;
    public string skinCategory;

    public void SwitchSkin(Team team)
    {
        spriteLibrary.spriteLibraryAsset = spriteLibraryArray[(int)team];
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
    }

    private void Start()
    {
        poc = GetComponent<PlayerObjectController>();
        audioSource = GetComponent<AudioSource>();
        //GameModeManager.instance.AddPlayer(this); // used for resetting game
    }

    private void FixedUpdate()
    {
        if (SceneManager.GetActiveScene().name == "Game")
        {
            if (isLocalPlayer)
            {
                Movement();
            }
        }
    }

    private void Update()
    {
        if (SceneManager.GetActiveScene().name == "Game")
        {
            if (!hasSpawned)
            {
                // Spawns player with knife, sets position, team, and sprite
                Debug.Log("Spawning");
                weaponInfo.setDefault();
                SetPosition();
                SetTeam();
                SwitchSkin(team);
                health = 10f;
                hasSpawned = true;
            }

            if (isLocalPlayer)
            {

                // temp test code
                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    SwitchSkin(Team.Green);
                }
                if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    SwitchSkin(Team.Orange);
                }
                if (Input.GetKeyDown(KeyCode.Alpha3))
                {
                    SwitchSkin(Team.Red);
                }
                if (Input.GetKeyDown(KeyCode.Alpha4))
                {
                    SwitchSkin(Team.White);
                }
                // end of test

                // Check if you are firing your weapon and if the cooldown is 0
                if (Input.GetButtonDown("Fire1") && cooldownTimer <= 0f)
                {
                    // Camera Shake
                    if (weaponInfo.nAmmo > 0)
                        CameraShaker.ShootCameraShake(5.0f);

                    // Start firing if the fire button is pressed down and this weapon is automatic
                    if (weaponInfo.isAuto)
                    {
                        // Set the isFiring flag to true and start firing
                        isFiring = true;
                        StartCoroutine(ContinuousFire());
                    }
                    else
                    {
                        // Fire a single shot
                        cooldownTimer = weaponInfo.cooldown;
                        Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
                        CmdShoot(shootPoint.position, mousePos);
                    }
                }
                else if (Input.GetButtonUp("Fire1") && weaponInfo.isAuto)
                {
                    // Stop firing if the fire button is released and this weapon is automatic
                    isFiring = false;
                    StopCoroutine(ContinuousFire());
                }

                // updates weapon cooldown timer
                cooldownTimer -= Time.deltaTime;
                if (cooldownTimer < 0) cooldownTimer = 0;
            }

            //TEST: player dies when pressing P
            if (Input.GetKeyDown(KeyCode.P)) GameModeManager.instance.PlayerDied(this);
        }
    }

    private IEnumerator ContinuousFire()
    {
        while (isFiring && cooldownTimer <= 0f)
        {
            // Fire a shot and wait for the cooldown timer to expire
            cooldownTimer = weaponInfo.cooldown;
            Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
            CmdShoot(shootPoint.position, mousePos);
            if (weaponInfo.nAmmo > 0)
                CameraShaker.ShootCameraShake(5.0f);
            yield return new WaitForSeconds(cooldownTimer);
        }
    }

    public void SetPosition()
    {
        // Sets the inital Spawn Position for all four characters
        //REFACTOR: Spawn points are gameObjects from the map class rather than vector3
        Debug.Log("Set POS ID: " + poc.PlayerIdNumber);
        if (poc.PlayerIdNumber == 1)
            transform.position = new Vector3(22.5f, 22.5f, 0.0f);
        if (poc.PlayerIdNumber == 2)
            transform.position = new Vector3(-22.5f, -22.5f, 0.0f);
        if (poc.PlayerIdNumber == 3)
            transform.position = new Vector3(-22.5f, 22.5f, 0.0f);
        if (poc.PlayerIdNumber == 4)
            transform.position = new Vector3(22.5f, -22.5f, 0.0f);
    }

    public void SetTeam()
    {
        //sets your team at the start of the game
        if (poc.PlayerIdNumber == 1)
            team = Team.Green;
        else if (poc.PlayerIdNumber == 2)
            team = Team.Red;
        else if (poc.PlayerIdNumber == 3)
            team = Team.Orange;
        else if (poc.PlayerIdNumber == 4) team = Team.White;
        //Refactor: get rid of extra team variable and only use PlayerController Team
        GetComponent<PlayerWeaponController>().team = team;
    }

    public void Movement()
    {
        float xDirection = Input.GetAxis("Horizontal");
        float yDirection = Input.GetAxis("Vertical");

        Vector3 mousePosition = Input.mousePosition;
        if(cam != null)
            mousePosition = cam.ScreenToWorldPoint(mousePosition);

        if ((mousePosition.x > transform.position.x && spriteRenderer.flipX) ||
            (mousePosition.x < transform.position.x && !spriteRenderer.flipX))
        {
            //Toggle sprite flip when your mouse moves across the character
            //spriteRenderer.flipX = !spriteRenderer.flipX;
            //weapon.transform.localScale = new Vector3(1, -weapon.transform.localScale.y, 1);
            CmdFlipPlayer();
            Debug.Log("Flipping" + team.ToString());
        }

        Vector2 direction = (mousePosition - weapon.transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        weapon.transform.eulerAngles = new Vector3(0, 0, angle);

        Vector3 moveDirection = new Vector3(xDirection, yDirection, 0.0f);
        //animate player running if they are moving
        if(moveDirection != new Vector3(0, 0, 0))
        {
            playerAnimator.SetBool("isRunning", true);
        }
        else
        {
            playerAnimator.SetBool("isRunning", false);
        }
        //apply the movement
        rb.MovePosition(transform.position + moveDirection *
                        weaponInfo.speedOfPlayer *
                        Time.deltaTime);
        Physics2D.SyncTransforms();
    }

    [Command]
    void CmdFlipPlayer()
    {
        RpcFlipPlayer();
    }

    [ClientRpc]
    void RpcFlipPlayer()
    {
        if (isLocalPlayer)
        {
            spriteRenderer.flipX = !spriteRenderer.flipX;
            weapon.transform.localScale = new Vector3(1, -weapon.transform.localScale.y, 1);
        }
    }

    [ClientRpc]
    void RpcSpawnBulletTrail(Vector2 startPos, Vector2 endPos)
    {
        if (weaponInfo.isMelee)
        {
            AudioSource
                .PlayClipAtPoint(KnifeSound, startPos, AudioListener.volume);
        }
        else
        {
            if (weaponInfo.id == WeaponID.AK47)
            {
                AudioSource.PlayClipAtPoint(AK47ShotSound, startPos, AudioListener.volume);
            }
            if (weaponInfo.id == WeaponID.Uzi)
            {
                AudioSource.PlayClipAtPoint(UziShotSound, startPos, AudioListener.volume);
            }
            if (weaponInfo.id == WeaponID.Sniper)
            {
                AudioSource.PlayClipAtPoint(SniperShotSound, startPos, AudioListener.volume);
            }
            if (weaponInfo.id == WeaponID.Pistol)
            {
                AudioSource.PlayClipAtPoint(PistolShotSound, startPos, AudioListener.volume);
            }
        }
        if (
            !weaponInfo.isMelee &&
            weaponInfo.nAmmo > 0
        )
        {
            Instantiate(bulletParticle.GetComponent<ParticleSystem>(),
            startPos,
            Quaternion.FromToRotation(Vector2.up, endPos-startPos));
            weaponInfo.nAmmo--;
        }

        Vector2 newPoint = endPos + ((endPos - startPos).normalized * -0.2f);
        var hitParticleInstance =
            Instantiate(hitParticle.GetComponent<ParticleSystem>(),
            newPoint,
            Quaternion.identity);
    }

    [Command]
    public void CmdShoot(Vector2 shootPoint, Vector2 mousePos)
    {
        if (weaponInfo.nAmmo > 0)
        {
            Vector2 direction = (mousePos - shootPoint).normalized;
            RaycastHit2D hit = Physics2D.Raycast(shootPoint, direction, weaponInfo.range);

            var endPos = hit.point;

            if (hit.collider != null && !hit.collider.CompareTag("Uncolliable"))
            {
                if (hit.collider.gameObject.tag == "Player")
                {
                    hit.collider.gameObject.GetComponent<PlayerController>().TakeDamage(weaponInfo.damage);

                    AudioSource
                        .PlayClipAtPoint(HurtsSound[Random.Range(0, 1)],
                        hit.point,
                        AudioListener.volume);
                }

                if (hit.collider.gameObject.tag == "destroy")
                {
                    Tilemap collidableTileMap =
                        GameObject.Find("destroyPots").GetComponent<Tilemap>();

                    Debug.Log("Hit Pot");

                    Vector3Int potPos =
                        collidableTileMap.WorldToCell(hit.point);

                    RpcBreakPot(potPos);

                    collidableTileMap.SetTile(potPos, null);

                    AudioSource
                        .PlayClipAtPoint(breakSound,
                        hit.point,
                        AudioListener.volume);

                    //drops ammo on top of broken pot (fixed!)
                    GameObject ammoInstance =
                        Instantiate(ammo, hit.point, Quaternion.identity);

                    NetworkServer.Spawn(ammoInstance);
                }
            }
            else
            {
                endPos =
                    shootPoint +
                    direction *
                    weaponInfo.range;
            }
            RpcSpawnBulletTrail(shootPoint, endPos);
        }
        else if (!weaponInfo.isMelee)
            RpcPlayEmptySound(shootPoint);
    }

    [ClientRpc]
    void RpcPlayEmptySound(Vector2 startPos)
    {
        AudioSource.PlayClipAtPoint(emptySound, startPos, AudioListener.volume);
    }

    [Command]
    public void CmdPlayerDied()
    {
        // Call the PlayerDied function on the server
        GameModeManager.instance.PlayerDied(this);
    }

    public void TakeDamage(float damage)
    {
        if (!isServer) return;

        health -= damage;
        Debug.Log("Player took " + damage + " Damage");

        RpcHurtCameraShake();

        if (health <= 0)
        {
            RpcDie();
            StartCoroutine(DelayedCmdPlayerDied());
        }
        else
        {
            RpcHitColor();
        }
    }

    private IEnumerator DelayedCmdPlayerDied()
    {
        yield return new WaitForSeconds(5f);
        if (isOwned)
        {
            // If the object has authority (belongs to the local player), send a command to notify the server about the death
            CmdPlayerDied();
        }
        else
        {
            // If the object does not have authority, it's likely a remote player object, and we don't need to do anything on the client-side.
            // The server will handle the death logic, and the state will be synchronized to this client automatically.
            GameModeManager.instance.PlayerDied(this);
        }
    }

    [ClientRpc]
    void RpcHurtCameraShake()
    {
        if (isLocalPlayer)
        {
            CameraShaker.HurtCameraShake(5.0f);
        }
    }

    IEnumerator FlashSprite()
    {
        // makes player flash red when hit
        Color temp = spriteRenderer.color;
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = Color.white;
    }

    [ClientRpc]
    void RpcHitColor()
    {
        StartCoroutine(FlashSprite());
    }

    [ClientRpc]
    void RpcDie()
    {
        // gets int value of Team enum
        int teamVal = (int) team;

        int index = 5 *4 + teamVal;
        //spriteRenderer.sprite = spriteArray[index]; old way of sprites

        weaponInfo.nAmmo = 0;
        weaponInfo.range = 0;
        weaponInfo.damage = 0;
        weaponInfo.speedOfPlayer = 0;
        GetComponent<PlayerWeaponController>().enabled = false;
    }

    [Command]
    public void CmdReset()
    {
        //test
        RpcRespawn();
    }

    [ClientRpc]
    public void RpcRespawn()
    {
        SetPosition();
        health = 10f;
        weaponInfo.setDefault();
        spriteRenderer.color = Color.white; // prevents sprite from having the red damage on it forever

        GetComponent<PlayerWeaponController>().enabled = true;
    }

    [ClientRpc]
    void RpcBreakPot(Vector3Int potPos)
    {
        Tilemap collidableTileMap =
            GameObject.Find("destroyPots").GetComponent<Tilemap>();
        collidableTileMap.SetTile(potPos, null);
        AudioSource.PlayClipAtPoint(breakSound, potPos, AudioListener.volume);
    }
}
