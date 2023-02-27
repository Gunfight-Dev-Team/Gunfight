using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

public class PlayerMovementController : NetworkBehaviour
{
    public float Speed = 5.0f;
    public GameObject PlayerModel;

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
        transform.position = new Vector3(Random.Range(-5, 5), Random.Range(-15, 7), 0.0f);
    }

    public void Movement()
    {
        float xDirection = Input.GetAxis("Horizontal");
        float yDirection = Input.GetAxis("Vertical");

        Vector3 moveDirection = new Vector3(xDirection, yDirection, 0.0f);

        transform.position += moveDirection * Speed * Time.deltaTime;
    }
}
