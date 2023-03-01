using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using static UnityEngine.GraphicsBuffer;
using TMPro;

public class CameraController : NetworkBehaviour
{
    public GameObject cameraHolder;

    public Vector3 offset;    // Offset to apply to camera position

    public float deadzoneRadius;

    public float damping;

    private Vector3 velocity = Vector3.zero;

    public override void OnStartAuthority()
    {
        cameraHolder.SetActive(true);
    }

    public void Update()
    {
        if (SceneManager.GetActiveScene().name == "Game")
        {

            float distance = Vector3.Distance(cameraHolder.transform.position, transform.position);

            if (distance > deadzoneRadius)
            {
                Vector3 smoothedPosition = Vector3.SmoothDamp(cameraHolder.transform.position, transform.position, ref velocity, damping);
                offset = Vector3.Lerp(cameraHolder.transform.position, smoothedPosition, damping);
                cameraHolder.transform.position = Vector3.Lerp(cameraHolder.transform.position, smoothedPosition, damping);
            }
            else
            {
                cameraHolder.transform.position = offset;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, deadzoneRadius);
    }
}
