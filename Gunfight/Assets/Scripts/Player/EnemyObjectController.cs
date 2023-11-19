using System.Collections;
using UnityEngine;
using Mirror;

public class EnemyObjectController : NetworkBehaviour
{
    public Pathfinding.AIDestinationSetter target;
    public Pathfinding.AIPath path;
    public SpriteRenderer spriteRenderer;

    public float health;

    public float speed;
    public float speedOffset = 0.1f;
    public float speedMultipiler = 0.5f;
    public GameObject closestPlayer;


    private GameObject[] players;

    private Vector3 previousPosition;

    void Start()
    {
        health = 10.0f;
        players = GameObject.FindGameObjectsWithTag("Player");
        target.target = GameObject.FindGameObjectWithTag("Player").transform;
        InvokeRepeating("FindClosest", 0f, 3f);
        path.maxSpeed *= speed + Random.Range(-speedOffset,speedOffset);
        speed = path.maxSpeed;
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

    public void updateSpeed()
    {
        float newSpeed = speed * (1 + speedMultipiler) + Random.Range(-speedOffset, speedOffset);
        path.maxSpeed = newSpeed;
        speed = path.maxSpeed;
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

    public void TakeDamage(float damage)
    {

        health -= damage;
        Debug.Log("Zombie took " + damage + " Damage");

        if (health <= 0)
        {
            RpcDie();
        }
        else
        {
            RpcHitColor();
        }
    }

    [Command]
    public void hitPlayer()
    {
        //TODO: attack players
    }

    void RpcDie()
    {
        spriteRenderer.enabled = false;
        GameModeManager.Instance.currentNumberOfEnemies--;
        Destroy(gameObject);
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
}
