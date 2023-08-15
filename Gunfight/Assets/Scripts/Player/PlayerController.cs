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
using UnityEngine.UI;


public enum Team
{
    Green,
    Red,
    Orange,
    White
}

public class PlayerController : NetworkBehaviour
{
    public WeaponInfo weaponInfo;
    public int grenades;

    private bool hasSpawned = false;

    public Rigidbody2D rb;

    public Camera cam;

    public PlayerObjectController poc;

    [SerializeField] 
    public Team team;

    //Sprite

    public SpriteRenderer spriteRenderer;

    public List<Sprite> spriteArray;

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

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        // Add an AudioListener component to the local player object
        //gameObject.AddComponent<AudioListener>();
    }

    private void Start()
    {
        //gameObject.SetActive(false);
        poc = GetComponent<PlayerObjectController>();
        LoadSprite();
        GetComponent<PlayerWeaponController>().spriteArray = spriteArray;
        audioSource = GetComponent<AudioSource>();
        //GameModeManager.instance.AddPlayer(this);
    }

    void LoadSprite()
    {
        AsyncOperationHandle<Sprite[]> spriteHandle =
            Addressables
                .LoadAssetAsync<Sprite[]>("Assets/Art/Player/Player_AK47.png");
        spriteHandle.WaitForCompletion();
        spriteHandle.Completed += LoadSpritesWhenReady;
        spriteHandle =
            Addressables
                .LoadAssetAsync<Sprite[]>("Assets/Art/Player/Player_Knife.png");
        spriteHandle.Completed += LoadSpritesWhenReady;
        spriteHandle =
            Addressables
                .LoadAssetAsync
                <Sprite[]>("Assets/Art/Player/Player_Pistol.png");
        spriteHandle.Completed += LoadSpritesWhenReady;
        spriteHandle =
            Addressables
                .LoadAssetAsync
                <Sprite[]>("Assets/Art/Player/Player_Sniper.png");
        spriteHandle.Completed += LoadSpritesWhenReady;
        spriteHandle =
            Addressables
                .LoadAssetAsync<Sprite[]>("Assets/Art/Player/Player_Uzi.png");
        spriteHandle.Completed += LoadSpritesWhenReady;
        spriteHandle =
            Addressables
                .LoadAssetAsync<Sprite[]>("Assets/Art/Player/Player_Death.png");
        spriteHandle.Completed += LoadSpritesWhenReady;
    }

    void LoadSpritesWhenReady(AsyncOperationHandle<Sprite[]> handleToCheck)
    {
        if (handleToCheck.Status == AsyncOperationStatus.Succeeded)
        {
            spriteArray.AddRange(handleToCheck.Result);
        }
    }

    private void FixedUpdate()
    {
        if (SceneManager.GetActiveScene().name == "Game")
        {
            if (!hasSpawned)
            {
                // Resets player stats to have a knife
                //REFACTOR: move playerInfo setting to function
                weaponInfo.setDefault();
                SetPosition();
                SetTeam();
                SetSprite();
                health = 10f;
                hasSpawned = true;
            }

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
            if (isLocalPlayer)
            {
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
                        CmdShooting(shootPoint.position, mousePos);
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
            CmdShooting(shootPoint.position, mousePos);
            if (weaponInfo.nAmmo > 0)
                CameraShaker.ShootCameraShake(5.0f);
            yield return new WaitForSeconds(cooldownTimer);
        }
    }

    public void SetPosition()
    {
        // Sets the inital Spawn Position for all four characters
        //REFACTOR: Spawn points are gameObjects from the map class rather than vector3
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

    public void SetSprite()
    {
        // [ ] TODO: is it possible to make this more simple?
        var teamArray =
            new Dictionary<Team, int>()
            {
                { Team.Green, 0 },
                { Team.Orange, 1 },
                { Team.Red, 2 },
                { Team.White, 3 }
            };

        // change sprite
        int index = 4 + teamArray[team];
        spriteRenderer.sprite = spriteArray[index];
    }

    public void Movement()
    {
        float xDirection = Input.GetAxis("Horizontal");
        float yDirection = Input.GetAxis("Vertical");

        // if (xDirection != 0.0f || yDirection != 0.0f)
        // {
        //     if (PlayerInfo.isMelee)
        //     {
        //         AudioSource
        //             .PlayClipAtPoint(Walk_4,
        //             PlayerModel.transform.position,
        //             AudioListener.volume);
        //     }
        //     else
        //     {
        //         if (
        //             PlayerInfo.weaponID ==
        //             WeaponID.AK47
        //         )
        //         {
        //             AudioSource
        //                 .PlayClipAtPoint(Walk_2,
        //                 PlayerModel.transform.position,
        //                 AudioListener.volume);
        //         }
        //         if (
        //             PlayerInfo.weaponID ==
        //             WeaponID.Uzi ||
        //             PlayerInfo.weaponID ==
        //             WeaponID.Pistol
        //         )
        //         {
        //             AudioSource
        //                 .PlayClipAtPoint(Walk_3,
        //                 PlayerModel.transform.position,
        //                 AudioListener.volume);
        //         }
        //         if (
        //             PlayerInfo.weaponID ==
        //             WeaponID.Sniper
        //         )
        //         {
        //             AudioSource
        //                 .PlayClipAtPoint(Walk_1,
        //                 PlayerModel.transform.position,
        //                 AudioListener.volume);
        //         }
        //     }
        // }
        Vector3 mousePosition = Input.mousePosition;
        if(cam != null)
            mousePosition = cam.ScreenToWorldPoint(mousePosition);

        Vector2 direction =
            new Vector2(mousePosition.x - transform.position.x,
                mousePosition.y - transform.position.y);

        transform.up = direction;

        Vector3 moveDirection = new Vector3(xDirection, yDirection, 0.0f);

        rb.MovePosition(transform.position + moveDirection *
                        weaponInfo.speedOfPlayer *
                        Time.deltaTime);
        Physics2D.SyncTransforms();
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
            transform.rotation);
            weaponInfo.nAmmo--;
        }

        Vector2 newPoint = endPos + ((endPos - startPos).normalized * -0.2f);
        var hitParticleInstance =
            Instantiate(hitParticle.GetComponent<ParticleSystem>(),
            newPoint,
            Quaternion.identity);
    }

    [Command]
    public void CmdShooting(Vector3 shootPoint, Vector2 mousePos)
    {
        if (weaponInfo.nAmmo > 0)
        {
            Vector2 direction = (mousePos - (Vector2)shootPoint).normalized;
            RaycastHit2D hit = Physics2D.Raycast(shootPoint, transform.up, weaponInfo.range);

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
                    transform.up *
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
        // [ ] TODO: is it possible to make this more simple?
        var teamArray =
            new Dictionary<Team, int>()
            {
                { Team.Green, 0 },
                { Team.Red, 1 },
                { Team.Orange, 2 },
                { Team.White, 3 }
            };

        int index = 5 * 4 + teamArray[team];
        spriteRenderer.sprite = spriteArray[index];

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
        SetSprite();
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
