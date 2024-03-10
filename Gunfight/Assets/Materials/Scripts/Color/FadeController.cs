using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeController : MonoBehaviour
{
    private Material material;
    private float cutAmount;
    private bool isDown;

    private void Start()
    {
        material = GetComponent<SpriteRenderer>().material;
        cutAmount = material.GetFloat("_FadeAmount");
    }

    private void Update()
    {
        if (cutAmount <= 0)
        {
            isDown = false;
        }
        else if (cutAmount >= 1)
        {
            isDown = true;
        }

        if (isDown)
        {
            cutAmount -= Time.deltaTime * 0.4f;
            material.SetFloat("_FadeAmount", cutAmount);
        }
        else
        {
            cutAmount += Time.deltaTime * 0.4f;
            material.SetFloat("_FadeAmount", cutAmount);
        }
    }
}
