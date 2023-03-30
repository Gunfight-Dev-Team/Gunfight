using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : MonoBehaviour
{
    public GameObject explosionEffect;
    public float explosionDelay = 3f;
    public AudioClip explosionSound;


    private void Start()
    {
        StartCoroutine(ExplodeAfterDelay());
    }

    IEnumerator ExplodeAfterDelay()
    {
        yield return new WaitForSeconds(explosionDelay);
        Explode();
    }

    void Explode()
    {
        Instantiate(explosionEffect, transform.position, Quaternion.identity);
        AudioSource.PlayClipAtPoint(explosionSound, transform.position, Random.Range(0.8f, 1.2f));
        Destroy(gameObject);
    }
}