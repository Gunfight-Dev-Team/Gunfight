using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using static UnityEngine.GraphicsBuffer;
using TMPro;
using UnityEngine.EventSystems;

public class CameraController : NetworkBehaviour
{
    public GameObject cameraHolder;

    public Rigidbody2D rb;

    public Camera cam;

    public GameObject target;

    public Vector3 offset;    // Offset to apply to camera position

    public float deadzoneRadius;

    public float mouseRangeRadius;

    public float damping;

    private Vector3 velocity = Vector3.zero;

    private Vector3 trackedPosition;

    public override void OnStartAuthority()
    {
        cameraHolder.SetActive(true);
    }

    public void FixedUpdate()
    {
        if (SceneManager.GetActiveScene().name == "Game")
        {

            float mouseDistance = Vector3.Distance(cameraHolder.transform.position, trackedPosition);

            Vector3 mousePosition = Input.mousePosition;
            mousePosition = cam.ScreenToWorldPoint(mousePosition);

            float curRadius = mouseRangeRadius / (1 / mouseDistance);
            
            float subDistances = curRadius / mouseDistance;
            trackedPosition.x = ((1.0f - subDistances)* target.transform.position.x + (subDistances*mousePosition.x));
            trackedPosition.y = ((1.0f - subDistances) * target.transform.position.y + (subDistances * mousePosition.y));

            float distance = Vector3.Distance(cameraHolder.transform.position, trackedPosition);

            if (distance > deadzoneRadius)
            {
                //Vector3 smoothedPosition = Vector3.SmoothDamp(cameraHolder.transform.position, trackedPosition, ref velocity, damping);
                Vector3 smoothedPosition = Vector3.Lerp(cameraHolder.transform.position, trackedPosition, 10f*Time.deltaTime);
                rb.MovePosition(smoothedPosition);
                //cameraHolder.transform.position = smoothedPosition;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(cameraHolder.transform.position, deadzoneRadius);
    }
}
