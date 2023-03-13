using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Mirror;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

public enum Team
{
    Green,
    Red,
    Orange,
    White
}

public class PlayerMovementController : NetworkBehaviour
{
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

    [SyncVar]
    public float health = 10f;

    public bool isDead = false;

    public GameObject hitParticle;

    public GameObject bulletParticle;

    private Vector2 mousePos;

    public AudioClip gunshotSound;
    private AudioSource audioSource;

    private void Start()
    {
        PlayerModel.SetActive(false);
        poc = GetComponent<PlayerObjectController>();
        LoadSprite();
        GetComponent<PlayerWeaponController>().spriteArray = spriteArray;
        audioSource = GetComponent<AudioSource>();
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
            if (PlayerModel.activeSelf == false)
            {
                if (health > 8) PlayerModel.SetActive(true);
                SetPosition();
                SetTeam();
                SetSprite();
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
                if (Input.GetButtonDown("Fire1") && cooldownTimer <=0f)
                {
                    cooldownTimer = PlayerModel.GetComponent<PlayerInfo>().cooldown;
                    CmdShooting(shootPoint.position);
                }
                cooldownTimer -= Time.deltaTime;
            }
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
                { Team.Red, 1 },
                { Team.Orange, 2 },
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
        AudioSource.PlayClipAtPoint(gunshotSound, startPos);
        var trail = Instantiate(bulletTrail, startPos, Quaternion.identity);
        var trailScript = trail.GetComponent<BulletTrail>();
        trailScript.SetTargetPosition (endPos);
        if (isServer) NetworkServer.Spawn(trail);

        Instantiate(bulletParticle.GetComponent<ParticleSystem>(),
        startPos,
        PlayerModel.transform.rotation);
        var hitParticleInstance =
            Instantiate(hitParticle.GetComponent<ParticleSystem>(),
            endPos,
            Quaternion.identity);
    }

    [Command]
    public void CmdShooting(Vector3 shootPoint)
    {
        Debug.Log("mouse pressed");
        Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mousePos - (Vector2) shootPoint).normalized;
        RaycastHit2D hit =
            Physics2D
                .Raycast(shootPoint, PlayerModel.transform.up, PlayerModel.GetComponent<PlayerInfo>().range);

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
                    .TakeDamage(PlayerModel.GetComponent<PlayerInfo>().damage);
            }
        }
        else
        {
            endPos = shootPoint + PlayerModel.transform.up * PlayerModel.GetComponent<PlayerInfo>().range;
        }
        RpcSpawnBulletTrail (shootPoint, endPos);
    }

    public void TakeDamage(float damage)
    {
        if (!isServer) return;

        health -= damage;
        Debug.Log("Player took " + damage + " Damage");

        if (health <= 0)
            RpcDie();
        else
        {
            RpcHitColor();
        }
    }

    IEnumerator FlashSprite()
    {
        Color temp = spriteRenderer.color;
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = temp;
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
        this.GetComponent<PlayerWeaponController>().enabled = false;
        this.enabled = false;
    }
}
