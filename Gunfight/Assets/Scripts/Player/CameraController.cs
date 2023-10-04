using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class CameraController : MonoBehaviour
{
    public Rigidbody2D rb;

    public Camera cam;

    public PlayerController target;

    public Vector3 offset;    // Offset to apply to camera position

    public float deadzoneRadius;

    public float mouseRangeRadius;

    public float damping;

    private Vector3 velocity = Vector3.zero;

    private Vector3 trackedPosition;

    public BoxCollider2D boundBox;

    private Vector3 minBounds;

    private Vector3 maxBounds;

    private float halfHeight;

    private float halfWidth;

    public void Start()
    {
        target = GameObject.Find("LocalGamePlayer").GetComponent<PlayerController>();
        target.cam = cam;
        target.CameraShaker = transform.Find("Camera").GetComponent<CameraShaker>();
        minBounds = boundBox.bounds.min;
        maxBounds = boundBox.bounds.max;
        halfHeight = cam.orthographicSize;
        halfWidth = halfHeight * Screen.width / Screen.height;
    }

    public void FixedUpdate()
    {
        if (SceneManager.GetActiveScene().name != "Lobby")
        {
            float clampedX = Mathf.Clamp(transform.position.x, minBounds.x + halfWidth, maxBounds.x - halfWidth);
            float clampedY = Mathf.Clamp(transform.position.y, minBounds.y + halfHeight, maxBounds.y - halfHeight);

            Vector3 clampedPos = new Vector3(clampedX, clampedY, transform.position.z);

            float mouseDistance = Vector3.Distance(clampedPos, trackedPosition);

            Vector3 mousePosition = Input.mousePosition;
            mousePosition = cam.ScreenToWorldPoint(mousePosition);

            float curRadius = mouseRangeRadius / (1 / mouseDistance);

            float subDistances = curRadius / mouseDistance;
            trackedPosition.x = ((1.0f - subDistances) * target.transform.position.x + (subDistances * mousePosition.x));
            trackedPosition.y = ((1.0f - subDistances) * target.transform.position.y + (subDistances * mousePosition.y));

            float distance = Vector3.Distance(clampedPos, trackedPosition);

            if (distance > deadzoneRadius)
            {
                //Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, trackedPosition, ref velocity, damping);
                Vector3 smoothedPosition = Vector3.Lerp(clampedPos, trackedPosition, 10f * Time.deltaTime);
                rb.MovePosition(smoothedPosition);
                //transform.position = smoothedPosition;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, deadzoneRadius);
    }
}
