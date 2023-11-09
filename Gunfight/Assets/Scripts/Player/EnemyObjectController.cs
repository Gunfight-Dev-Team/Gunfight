using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Steamworks;
using UnityEngine.SceneManagement;

public class EnemyObjectController : NetworkBehaviour
{
    public Pathfinding.AIDestinationSetter target;
    public SpriteRenderer spriteRenderer;

    [SyncVar(hook = nameof(RoundUpdate))] public float speed = 1f;
    public float speedOffset = 0.1f;
    public GameObject closestPlayer;
    //private float closestDistance = float.MaxValue;

    //private GameObject[] players;
    //private Rigidbody2D rb;

    private Vector3 previousPosition;

    void Start()
    {
        //players = GameObject.FindGameObjectsWithTag("Player");
        //rb = GetComponent<Rigidbody2D>();
        //closestPlayer = GameObject.FindGameObjectWithTag("Player");
        target.target = GameObject.FindGameObjectWithTag("Player").transform;
        //InvokeRepeating("FindClosest", 0f, 3f);

        previousPosition = transform.position;
    }

    void Update()
    {
        updateFlip();

    }

    /*    private void FindClosest()
        {

            foreach (GameObject player in players)
            {
                float distance = Vector3.Distance(transform.position, player.transform.position);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPlayer = player;
                }
            }

        }*/

    public void RoundUpdate(float OldValue, float NewValue)
    {
        // TODO: if round ends, increase speed
        if (isServer)
        {
            speed = NewValue;
        }
    }

    [ClientRpc]
    private void increaseSpeed(float OldValue)
    {
        float newSpeed = OldValue * (1 + speedOffset);
        RoundUpdate(OldValue, newSpeed);
    }

    void updateFlip()
    {
        // Calculate the change in position
        Vector3 deltaPosition = transform.position - previousPosition;

        // Update the previous position for the next frame
        previousPosition = transform.position;

        if (deltaPosition.x < 0)
        {
            spriteRenderer.flipX = true;
        }
        else
        {
            spriteRenderer.flipX = false;
        }
    }
}
