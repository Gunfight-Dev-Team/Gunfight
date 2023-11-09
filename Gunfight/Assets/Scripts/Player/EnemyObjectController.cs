using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Steamworks;
using UnityEngine.SceneManagement;

public class EnemyObjectController : NetworkBehaviour
{
    public Pathfinding.AIDestinationSetter target;

    //[SyncVar(hook = nameof(RoundUpdate))] public float speed = 1f;
    //public float speedOffset = 0.1f;
    public GameObject closestPlayer;
    //private float closestDistance = float.MaxValue;

    //private GameObject[] players;
    //private Rigidbody2D rb;

    void Start()
    {
        //players = GameObject.FindGameObjectsWithTag("Player");
        //rb = GetComponent<Rigidbody2D>();
        //closestPlayer = GameObject.FindGameObjectWithTag("Player");
        target.target = GameObject.FindGameObjectWithTag("Player").transform;
        //InvokeRepeating("FindClosest", 0f, 3f);
    }

    void Update()
    {
        /*transform.up = (closestPlayer.transform.position - transform.position).normalized;

        rb.MovePosition(transform.position + transform.up * speed * Time.deltaTime);
        Physics2D.SyncTransforms();*/
        //target.target = GameObject.FindGameObjectWithTag("Player").transform;

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

/*    public void RoundUpdate(float OldValue, float NewValue)
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
        float newSpeed = OldValue * (1 + Random.Range(-speedOffset, speedOffset));
        RoundUpdate(OldValue, newSpeed);
    }*/
}
