using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Mirror;
using Unity.Burst.CompilerServices;
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

public class PlayerMovementController : NetworkBehaviour
{
    public bool resetGame = false;

    public float Speed = 0.0f;

    public GameObject PlayerModel;

    public Rigidbody2D rb;

    public Camera cam;

    public PlayerObjectController poc;

    [SerializeField]
    public Team team;

    //Sprite
    public GameObject player;

    public SpriteRenderer spriteRenderer;

    public List<Sprite> spriteArray;

    //Shooting
    public Transform shootPoint;

    public float bulletTrailSpeed;

    public GameObject bulletTrail;

    public float cooldownTimer = 0;

    public bool isFiring;

    public CameraShaker CameraShaker;

    [SyncVar]
    public float health = 10f;

    public bool alive = true;

    public GameObject hitParticle;

    public GameObject bulletParticle;

    private Vector2 mousePos;

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

    [SerializeField] public GameObject ammo;

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        // Add an AudioListener component to the local player object
        PlayerModel.AddComponent<AudioListener>();
    }

    private void Start()
    {
        PlayerModel.SetActive(false);
        poc = GetComponent<PlayerObjectController>();
        LoadSprite();
        GetComponent<PlayerWeaponController>().spriteArray = spriteArray;
        audioSource = GetComponent<AudioSource>();
        GameModeManager.instance.AddPlayer(this);
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
            if (PlayerModel.activeSelf == false || resetGame == true)
            {
                PlayerModel.SetActive(true);
                SetPosition();
                SetTeam();
                SetSprite();
                health = 10f;
                resetGame = false;
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
                if (Input.GetButtonDown("Fire1") && cooldownTimer <= 0f)
                {
                    if (PlayerModel.GetComponent<PlayerInfo>().nAmmo > 0)
                        CameraShaker.ShootCameraShake(5.0f);

                    // Start firing if the fire button is pressed down and this weapon is automatic
                    if (PlayerModel.GetComponent<PlayerInfo>().isAuto)
                    {
                        // Set the isFiring flag to true and start firing
                        isFiring = true;
                        StartCoroutine(ContinuousFire());
                    }
                    else
                    {
                        // Fire a single shot
                        cooldownTimer =
                            PlayerModel.GetComponent<PlayerInfo>().cooldown;
                        CmdShooting(shootPoint.position);
                    }
                }
                else if (
                    Input.GetButtonUp("Fire1") &&
                    PlayerModel.GetComponent<PlayerInfo>().isAuto
                )
                {
                    // Stop firing if the fire button is released and this weapon is automatic
                    isFiring = false;
                    StopCoroutine(ContinuousFire());
                }

                cooldownTimer -= Time.deltaTime;
                if (cooldownTimer < 0) cooldownTimer = 0;
            }

            if (Input.GetKeyDown(KeyCode.P)) GameModeManager.instance.PlayerDied(this);
        }
    }

    private IEnumerator ContinuousFire()
    {
        while (isFiring && cooldownTimer <= 0f)
        {
            // Fire a shot and wait for the cooldown timer to expire
            cooldownTimer = PlayerModel.GetComponent<PlayerInfo>().cooldown;
            CmdShooting(shootPoint.position);
            if (PlayerModel.GetComponent<PlayerInfo>().nAmmo > 0)
                CameraShaker.ShootCameraShake(5.0f);
            yield return new WaitForSeconds(cooldownTimer);
        }
    }

    public void SetPosition()
    {
        if (poc.PlayerIdNumber == 1)
            PlayerModel.transform.position = new Vector3(22.5f, 22.5f, 0.0f);
        if (poc.PlayerIdNumber == 2)
            PlayerModel.transform.position = new Vector3(-22.5f, -22.5f, 0.0f);
        if (poc.PlayerIdNumber == 3)
            PlayerModel.transform.position = new Vector3(-22.5f, 22.5f, 0.0f);
        if (poc.PlayerIdNumber == 4)
            PlayerModel.transform.position = new Vector3(22.5f, -22.5f, 0.0f);
    }

    public void SetTeam()
    {
        if (poc.PlayerIdNumber == 1)
            team = Team.Green;
        else if (poc.PlayerIdNumber == 2)
            team = Team.Red;
        else if (poc.PlayerIdNumber == 3)
            team = Team.Orange;
        else if (poc.PlayerIdNumber == 4) team = Team.White;
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
        //     if (PlayerModel.GetComponent<PlayerInfo>().isMelee)
        //     {
        //         AudioSource
        //             .PlayClipAtPoint(Walk_4,
        //             PlayerModel.transform.position,
        //             AudioListener.volume);
        //     }
        //     else
        //     {
        //         if (
        //             PlayerModel.GetComponent<PlayerInfo>().weaponID ==
        //             WeaponID.AK47
        //         )
        //         {
        //             AudioSource
        //                 .PlayClipAtPoint(Walk_2,
        //                 PlayerModel.transform.position,
        //                 AudioListener.volume);
        //         }
        //         if (
        //             PlayerModel.GetComponent<PlayerInfo>().weaponID ==
        //             WeaponID.Uzi ||
        //             PlayerModel.GetComponent<PlayerInfo>().weaponID ==
        //             WeaponID.Pistol
        //         )
        //         {
        //             AudioSource
        //                 .PlayClipAtPoint(Walk_3,
        //                 PlayerModel.transform.position,
        //                 AudioListener.volume);
        //         }
        //         if (
        //             PlayerModel.GetComponent<PlayerInfo>().weaponID ==
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
        mousePosition = cam.ScreenToWorldPoint(mousePosition);

        Vector2 direction =
            new Vector2(mousePosition.x - PlayerModel.transform.position.x,
                mousePosition.y - PlayerModel.transform.position.y);

        PlayerModel.transform.up = direction;

        Vector3 moveDirection = new Vector3(xDirection, yDirection, 0.0f);

        rb
            .MovePosition(PlayerModel.transform.position +
            moveDirection *
            PlayerModel.GetComponent<PlayerInfo>().speedOfPlayer *
            Time.deltaTime);
        Physics2D.SyncTransforms();
        //PlayerModel.transform.position += moveDirection * Speed * Time.deltaTime;
    }

    [ClientRpc]
    void RpcSpawnBulletTrail(Vector2 startPos, Vector2 endPos)
    {
        if (PlayerModel.GetComponent<PlayerInfo>().isMelee)
        {
            AudioSource
                .PlayClipAtPoint(KnifeSound, startPos, AudioListener.volume);
        }
        else
        {
            if (PlayerModel.GetComponent<PlayerInfo>().weaponID == WeaponID.AK47
            )
            {
                AudioSource
                    .PlayClipAtPoint(AK47ShotSound,
                    startPos,
                    AudioListener.volume);
            }
            if (PlayerModel.GetComponent<PlayerInfo>().weaponID == WeaponID.Uzi)
            {
                AudioSource
                    .PlayClipAtPoint(UziShotSound,
                    startPos,
                    AudioListener.volume);
            }
            if (
                PlayerModel.GetComponent<PlayerInfo>().weaponID ==
                WeaponID.Sniper
            )
            {
                AudioSource
                    .PlayClipAtPoint(SniperShotSound,
                    startPos,
                    AudioListener.volume);
            }
            if (
                PlayerModel.GetComponent<PlayerInfo>().weaponID ==
                WeaponID.Pistol
            )
            {
                AudioSource
                    .PlayClipAtPoint(PistolShotSound,
                    startPos,
                    AudioListener.volume);
            }
        }
        if (
            !PlayerModel.GetComponent<PlayerInfo>().isMelee &&
            PlayerModel.GetComponent<PlayerInfo>().nAmmo > 0
        )
        {
            Instantiate(bulletParticle.GetComponent<ParticleSystem>(),
            startPos,
            PlayerModel.transform.rotation);
            PlayerModel.GetComponent<PlayerInfo>().nAmmo--;
        }

        Vector2 newPoint = endPos + ((endPos - startPos).normalized * -0.2f);
        var hitParticleInstance =
            Instantiate(hitParticle.GetComponent<ParticleSystem>(),
            newPoint,
            Quaternion.identity);
    }

    [Command]
    public void CmdShooting(Vector3 shootPoint)
    {
        if (PlayerModel.GetComponent<PlayerInfo>().nAmmo > 0)
        {
            Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
            Vector2 direction = (mousePos - (Vector2) shootPoint).normalized;
            RaycastHit2D hit =
                Physics2D
                    .Raycast(shootPoint,
                    PlayerModel.transform.up,
                    PlayerModel.GetComponent<PlayerInfo>().range);

            var endPos = hit.point;

            if (
                hit.collider != null && !hit.collider.CompareTag("Uncolliable") //&& hit.collider.CompareTag("Enemy")
            )
            {
                Debug.Log("hit");
                if (hit.collider.gameObject.tag == "Player")
                {
                    Debug.Log("Hit Player");
                    hit
                        .collider
                        .gameObject
                        .transform
                        .parent
                        .gameObject
                        .GetComponent<PlayerMovementController>()
                        .TakeDamage(PlayerModel
                            .GetComponent<PlayerInfo>()
                            .damage);

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

                    RpcBreakPot (potPos);

                    collidableTileMap.SetTile(potPos, null);

                    AudioSource
                        .PlayClipAtPoint(breakSound,
                        hit.point,
                        AudioListener.volume);

                    //drops ammo on top of broken pot (fixed!)
                    GameObject ammoInstance =
                        Instantiate(ammo, hit.point, Quaternion.identity);

                    NetworkServer.Spawn (ammoInstance);
                }
            }
            else
            {
                endPos =
                    shootPoint +
                    PlayerModel.transform.up *
                    PlayerModel.GetComponent<PlayerInfo>().range;
            }
            RpcSpawnBulletTrail (shootPoint, endPos);
        }
        else if (!PlayerModel.GetComponent<PlayerInfo>().isMelee)
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
            if (isOwned)
            {
                // If the object has authority (belongs to the local player), send a command to notify the server about the death
                CmdPlayerDied();
            }
            else
            {
                // If the object does not have authority, it's likely a remote player object, and we don't need to do anything on the client-side.
                // The server will handle the death logic, and the state will be synchronized to this client automatically.
               RpcRespawn();
            }
        }
        else
        {
            RpcHitColor();
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
        //gameObject.transform.Find("Player").gameObject.SetActive(false);
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

        PlayerModel.GetComponent<PlayerInfo>().nAmmo = 0;
        PlayerModel.GetComponent<PlayerInfo>().range = 0;
        PlayerModel.GetComponent<PlayerInfo>().damage = 0;
        PlayerModel.GetComponent<PlayerInfo>().speedOfPlayer = 0;
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
        PlayerModel.GetComponent<PlayerInfo>().nAmmo = 0;
        PlayerModel.GetComponent<PlayerInfo>().range = 0.5f;
        PlayerModel.GetComponent<PlayerInfo>().damage = 10;
        PlayerModel.GetComponent<PlayerInfo>().speedOfPlayer = 8;
        PlayerModel.GetComponent<PlayerInfo>().cooldown = 0.2f;
        PlayerModel.GetComponent<PlayerInfo>().isMelee = true;
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
