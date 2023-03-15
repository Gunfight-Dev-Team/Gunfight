using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Collectable : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnScaleChanged))]
    private Vector3 scale;

    public AudioClip collectSound;

    public int ammoToAdd = 10;

    private void Start()
    {
        // Set a random starting scale
        scale = transform.localScale;
    }

    private void Update()
    {
        // Scale the collectable up and down over time
        float scaleAmount = Mathf.Sin(Time.time * 5f) * 0.2f + 1f;
        scale = (Vector3.one/2) * scaleAmount;
    }

    private void OnScaleChanged(Vector3 oldScale, Vector3 newScale)
    {
        // Update the transform's local scale
        transform.localScale = newScale;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isServer) return;

        // Check if the player collided with this collectable
        PlayerInfo playerInfo = collision.GetComponent<PlayerInfo>();
        if (playerInfo != null)
        {
            AudioSource.PlayClipAtPoint(collectSound, transform.position, AudioListener.volume);
            // Add ammo to the player's count
            playerInfo.nAmmo += 10;

            // Destroy the collectable on all clients
            NetworkServer.Destroy(gameObject);
        }
    }
}
