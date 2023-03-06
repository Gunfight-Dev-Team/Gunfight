using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

public class PlayerMovementController : NetworkBehaviour
{
    public float Speed = 5.0f;
    public GameObject PlayerModel;
    public Rigidbody2D rb;
    public Camera cam;

    private Vector2 mousePos;

    private void Start()
    {
        PlayerModel.SetActive(false);
    }

    private void FixedUpdate()
    {
        if(SceneManager.GetActiveScene().name == "Game")
        {
            if(PlayerModel.activeSelf == false)
            {
                SetPosition();
                PlayerModel.SetActive(true);
            }
            
            if(isOwned)
            {
                Movement();
            }
        }
    }

    public void SetPosition()
    {
        PlayerModel.transform.position = new Vector3(Random.Range(0, 0), Random.Range(0, 0), 0.0f);
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

        rb.MovePosition(PlayerModel.transform.position + moveDirection * Speed * Time.deltaTime);
        //PlayerModel.transform.position += moveDirection * Speed * Time.deltaTime;

    }
}
