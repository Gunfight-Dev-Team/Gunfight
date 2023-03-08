using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class PlayerMovementController : NetworkBehaviour
{
    public float Speed = 5.0f;
    public GameObject PlayerModel;
    public Camera cam;
    public Transform shootPoint;
    public float bulletTrailSpeed;
    public GameObject bulletTrail;
    public float weaponRange = 10f;
    [SyncVar] public float health = 10f;



    private Vector2 mousePos;

    private void Start()
    {
        PlayerModel.SetActive(false);
    }

    private void Update()
    {
        if(SceneManager.GetActiveScene().name == "Game")
        {
            if(PlayerModel.activeSelf == false)
            {
                SetPosition();
                if (health >= 9)
                {
                    PlayerModel.SetActive(true);
                }
            }
            
            if(isOwned)
            {
                Shooting();
                Movement();
            }
        }
    }

    public void SetPosition()
    {
        PlayerModel.transform.position = new Vector3(Random.Range(-1, 1), Random.Range(-1, 1), 0.0f);
    }

    public void Movement()
    {
        float xDirection = Input.GetAxis("Horizontal");
        float yDirection = Input.GetAxis("Vertical");

        Vector3 mousePosition = Input.mousePosition;
        mousePosition = cam.ScreenToWorldPoint(mousePosition);

        Vector2 direction = new Vector2(
            mousePosition.x - PlayerModel.transform.position.x,
            mousePosition.y - PlayerModel.transform.position.y
        );

        PlayerModel.transform.up = direction;

        Vector3 moveDirection = new Vector3(xDirection, yDirection, 0.0f);

        PlayerModel.transform.position += moveDirection * Speed * Time.deltaTime;
     
    }

    [Command]
    void CmdShootRay()
    {
        RpcFireWeapon();
    }

    [ClientRpc]
    void RpcFireWeapon()
    {
        Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mousePos - (Vector2)shootPoint.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(shootPoint.position, direction, weaponRange);

        var trail = Instantiate(bulletTrail, shootPoint.position, PlayerModel.transform.rotation);

        var trailScript = trail.GetComponent<BulletTrail>();

        if (hit.collider != null) //&& hit.collider.CompareTag("Enemy")
        {
            trailScript.SetTargetPosition(hit.point);
            hit.collider.gameObject.SetActive(false);
            //hit.collider.gameObject.GetComponent<PlayerMovementController>().health -= 1;
        }
        else
        {
            var endPos = shootPoint.position + PlayerModel.transform.up * weaponRange;
            trailScript.SetTargetPosition(endPos);
        }
    }

    public void Shooting()
    {
        if(Input.GetButtonDown("Fire1"))
        {
            RpcFireWeapon();
        }
    }
}
