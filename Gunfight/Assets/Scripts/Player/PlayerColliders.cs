using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerColliders : MonoBehaviour
{

    public bool canPickup = false;
    public Collider2D OtherCollider;
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Weapon"))
        {
            canPickup = true;
            OtherCollider = other;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Weapon"))
        {
            canPickup = false;
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        // Check if the colliding object has the "Player" tag
        if (other.CompareTag("Weapon"))
        {
            OtherCollider = other;
            canPickup = true;
        }
    }
}
