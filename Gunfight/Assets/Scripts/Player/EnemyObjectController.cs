using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Steamworks;
using UnityEngine.SceneManagement;

public class EnemyObjectController : NetworkBehaviour
{
    public Pathfinding.AIDestinationSetter target;
    public Pathfinding.AIPath path;
    public SpriteRenderer spriteRenderer;

    public float speed;
    public float speedOffset = 0.0001f;
    public GameObject closestPlayer;


    private GameObject[] players;

    private Vector3 previousPosition;

    void Start()
    {
        players = GameObject.FindGameObjectsWithTag("Player");
        target.target = GameObject.FindGameObjectWithTag("Player").transform;
        InvokeRepeating("FindClosest", 0f, 3f);
        path.maxSpeed *= speed;
        previousPosition = transform.position;
    }

    void Update()
    {
        updateFlip();
    }

    private void FindClosest()
    {
        float closestDistance = float.MaxValue;
        foreach (GameObject player in players)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);
            
            if (distance < closestDistance)
            {
                closestDistance = distance;
                target.target = player.transform;
            }
        }

    }

    private void increaseSpeed(float OldValue)
    {
        float newSpeed = OldValue * (1 + speedOffset);
        path.maxSpeed = newSpeed;
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
