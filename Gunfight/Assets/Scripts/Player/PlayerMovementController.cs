using System.Collections;
using System.Collections.Generic;
using Mirror;
using Unity.Burst.CompilerServices;
using UnityEngine;
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
    public float Speed = 5.0f;

    public GameObject PlayerModel;

    public Rigidbody2D rb;

    public Camera cam;

    public PlayerObjectController poc;

    [SerializeField] public Team team;

    [SerializeField] public Sprite[] greenSprite;
    [SerializeField] public Sprite[] redSprite;
    [SerializeField] public Sprite[] orangeSprite;
    [SerializeField] public Sprite[] whiteSprite;
    public Sprite deadSprite;
    public GameObject player;
    public SpriteRenderer spriteRenderer;

    //Shooting
    public Transform shootPoint;
    public float bulletTrailSpeed;
    public GameObject bulletTrail;
    public float weaponRange = 10f;
    [SyncVar] public float health = 10f;
    public bool isDead = false;

    public GameObject hitParticle;
    public GameObject bulletParticle;

    private Vector2 mousePos;

    private void Start()
    {
        PlayerModel.SetActive(false);
        poc = GetComponent<PlayerObjectController>();
    }

    private void FixedUpdate()
    {
        if (SceneManager.GetActiveScene().name == "Game")
        {
            if (PlayerModel.activeSelf == false)
            {
                if (health > 8)
                    PlayerModel.SetActive(true);
                SetPosition();
                SetTeam();
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
                if (Input.GetButtonDown("Fire1"))
                {
                    CmdShooting(shootPoint.position);
                }
            }
        }
            if (isDead)
            spriteRenderer.sprite = deadSprite;
        if (team.Equals(Team.Green))
        {
            // [ ] TODO: change it to be using event intead of update
            if (player.GetComponent<PlayerInfo>().weaponID.Equals(WeaponID.AK47))
                spriteRenderer.sprite = greenSprite[0];
            else if (player.GetComponent<PlayerInfo>().weaponID.Equals(WeaponID.Knife))
                spriteRenderer.sprite = greenSprite[1];
            else if (player.GetComponent<PlayerInfo>().weaponID.Equals(WeaponID.Pistol))
                spriteRenderer.sprite = greenSprite[2];
            else if (player.GetComponent<PlayerInfo>().weaponID.Equals(WeaponID.Sniper))
                spriteRenderer.sprite = greenSprite[3];
            else if (player.GetComponent<PlayerInfo>().weaponID.Equals(WeaponID.Uzi))
                spriteRenderer.sprite = greenSprite[4];
        }

        if (team.Equals(Team.Red))
        {
            // [ ] TODO: change it to be using event intead of update
            if (player.GetComponent<PlayerInfo>().weaponID.Equals(WeaponID.AK47))
                spriteRenderer.sprite = redSprite[0];
            else if (player.GetComponent<PlayerInfo>().weaponID.Equals(WeaponID.Knife))
                spriteRenderer.sprite = redSprite[1];
            else if (player.GetComponent<PlayerInfo>().weaponID.Equals(WeaponID.Pistol))
                spriteRenderer.sprite = redSprite[2];
            else if (player.GetComponent<PlayerInfo>().weaponID.Equals(WeaponID.Sniper))
                spriteRenderer.sprite = redSprite[3];
            else if (player.GetComponent<PlayerInfo>().weaponID.Equals(WeaponID.Uzi))
                spriteRenderer.sprite = redSprite[4];
        }

        if (team.Equals(Team.Orange))
        {
            // [ ] TODO: change it to be using event intead of update
            if (player.GetComponent<PlayerInfo>().weaponID.Equals(WeaponID.AK47))
                spriteRenderer.sprite = orangeSprite[0];
            else if (player.GetComponent<PlayerInfo>().weaponID.Equals(WeaponID.Knife))
                spriteRenderer.sprite = orangeSprite[1];
            else if (player.GetComponent<PlayerInfo>().weaponID.Equals(WeaponID.Pistol))
                spriteRenderer.sprite = orangeSprite[2];
            else if (player.GetComponent<PlayerInfo>().weaponID.Equals(WeaponID.Sniper))
                spriteRenderer.sprite = orangeSprite[3];
            else if (player.GetComponent<PlayerInfo>().weaponID.Equals(WeaponID.Uzi))
                spriteRenderer.sprite = orangeSprite[4];
        }

        if (team.Equals(Team.White))
        {
            // [ ] TODO: change it to be using event intead of update
            if (player.GetComponent<PlayerInfo>().weaponID.Equals(WeaponID.AK47))
                spriteRenderer.sprite = whiteSprite[0];
            else if (player.GetComponent<PlayerInfo>().weaponID.Equals(WeaponID.Knife))
                spriteRenderer.sprite = whiteSprite[1];
            else if (player.GetComponent<PlayerInfo>().weaponID.Equals(WeaponID.Pistol))
                spriteRenderer.sprite = whiteSprite[2];
            else if (player.GetComponent<PlayerInfo>().weaponID.Equals(WeaponID.Sniper))
                spriteRenderer.sprite = whiteSprite[3];
            else if (player.GetComponent<PlayerInfo>().weaponID.Equals(WeaponID.Uzi))
                spriteRenderer.sprite = whiteSprite[4];
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
        {
            team = Team.Green;
            spriteRenderer.sprite = greenSprite[1];
        }
        else if (poc.PlayerIdNumber == 2)
        {
            team = Team.Red;
            spriteRenderer.sprite = redSprite[1];
        }
        else if (poc.PlayerIdNumber == 3)
        {
            team = Team.Orange;
            spriteRenderer.sprite = orangeSprite[1];
        }
        else if (poc.PlayerIdNumber == 4)
        {
            team = Team.White;
            spriteRenderer.sprite = whiteSprite[1];
        }
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

        rb.MovePosition(PlayerModel.transform.position + moveDirection * Speed * Time.deltaTime);
        Physics2D.SyncTransforms();
        //PlayerModel.transform.position += moveDirection * Speed * Time.deltaTime;
    }


    [ClientRpc]
    void RpcSpawnBulletTrail(Vector2 startPos, Vector2 endPos)
    {
        var trail = Instantiate(bulletTrail, startPos, Quaternion.identity);
        var trailScript = trail.GetComponent<BulletTrail>();
        trailScript.SetTargetPosition(endPos);
        if(isServer)
            NetworkServer.Spawn(trail);

        Instantiate(bulletParticle.GetComponent<ParticleSystem>(), startPos, PlayerModel.transform.rotation);
        var hitParticleInstance = Instantiate(hitParticle.GetComponent<ParticleSystem>(), endPos, Quaternion.identity);
    }

    [Command]
    public void CmdShooting(Vector3 shootPoint)
    {
            Debug.Log("mouse pressed");
            Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
            Vector2 direction = (mousePos - (Vector2)shootPoint).normalized;
            RaycastHit2D hit = Physics2D.Raycast(shootPoint, PlayerModel.transform.up, weaponRange);

            var endPos = hit.point;

            if (hit.collider != null && !hit.collider.CompareTag("Uncolliable")) //&& hit.collider.CompareTag("Enemy")
            {
                Debug.Log("hit");
                if (hit.collider.gameObject.tag == "Player")
                {
                    Debug.Log("Hit Player");
                    hit.collider.gameObject.transform.parent.gameObject.GetComponent<PlayerMovementController>().TakeDamage(2);
                }
            }
            else
            {
                endPos = shootPoint + PlayerModel.transform.up * weaponRange;
            }
            RpcSpawnBulletTrail(shootPoint, endPos);
    }

    public void TakeDamage(float damage)
    {
        if (!isServer)
            return;

        health -= damage;

        if (health <= 0)
            RpcDie();
        else
        {
            StartCoroutine(FlashSprite());
        }
    }

    IEnumerator FlashSprite()
    {
        Color temp = spriteRenderer.color;
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.2f);
        spriteRenderer.color = temp;
    }

    [ClientRpc]
    void RpcDie()
    {
        gameObject.transform.Find("Player").gameObject.SetActive(false);
    }
}
