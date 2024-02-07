using UnityEngine;
using UnityEngine.UI;

public class menuButtonTween : MonoBehaviour
{
    public float hoverDuration;
    public float hoverScaleFactor; // Public variable for the scaling factor
    public Color hoverColor; // Public variable for the hover color
    public GameObject uiShotPrefab; // Reference to the UIshot prefab

    private Transform spriteObject;
    private Transform textObject;

    private Vector3 originalScaleSprite;
    private Vector3 originalScaleText;

    private Color originalLastChildColor; // Store the original color of the last child

    private int hoverTweenId; // Store the tween ID for the hover effect

    void Start()
    {
        // Find sprite and text objects under the current GameObject's children
        spriteObject = transform.Find("sprite");
        textObject = transform.Find("Text (TMP)");

        if (spriteObject == null || textObject == null)
        {
            Debug.LogError("Sprite or Text object not found. Make sure to set the correct names.");
        }

        // Store the original scales for later use
        originalScaleSprite = spriteObject.localScale;
        originalScaleText = textObject.localScale;

        // Get the original color of the last child in the sprite component
        Transform lastChild = spriteObject.GetChild(spriteObject.childCount - 1);
        if (lastChild != null)
        {
            Image lastChildRenderer = lastChild.GetComponent<Image>();
            if (lastChildRenderer != null)
            {
                originalLastChildColor = lastChildRenderer.color;
            }
        }
    }

    public void onHover()
    {
        // Cancel any existing hover effect to avoid conflicts
        LeanTween.cancel(gameObject, hoverTweenId);

        // Loop the scaling effect
        hoverTweenId = LeanTween.value(gameObject, 1f, hoverScaleFactor, hoverDuration)
            .setLoopPingPong()
            .setOnUpdate((float val) =>
            {
                // Scale the button
                LeanTween.scale(gameObject, new Vector3(val, val, val), 0);

                // Scale the sprite
                LeanTween.scale(spriteObject.gameObject, originalScaleSprite * val, 0);

                // Scale the text
                LeanTween.scale(textObject.gameObject, originalScaleText * val, 0);
            })
            .id; // Store the tween ID

        // Rotate the sprite
        LeanTween.rotateZ(gameObject, Random.Range(0, 2) == 0 ? -2.5f : 2.5f, hoverDuration)
            .setEase(LeanTweenType.easeOutQuad);

        // Change the color of the last child in the sprite component
        Transform lastChild = spriteObject.GetChild(spriteObject.childCount - 1);
        if (lastChild != null)
        {
            Image lastChildRenderer = lastChild.GetComponent<Image>();
            if (lastChildRenderer != null)
            {
                lastChildRenderer.color = hoverColor;
            }
        }
    }

    public void onPointerExit()
    {
        // Reset the scaling when the pointer exits
        LeanTween.scale(gameObject, Vector3.one, hoverDuration)
            .setEase(LeanTweenType.easeOutElastic);

        // Reset the sprite scaling and rotation
        LeanTween.scale(gameObject, originalScaleSprite, hoverDuration)
            .setEase(LeanTweenType.easeOutElastic);

        LeanTween.rotateZ(gameObject, 0f, hoverDuration)
            .setEase(LeanTweenType.easeOutQuad);

        // Reset the text scaling and color
        LeanTween.scale(textObject.gameObject, originalScaleText, hoverDuration)
            .setEase(LeanTweenType.easeOutElastic);

        // Cancel the ongoing hover effect
        LeanTween.cancel(gameObject, hoverTweenId);

        // Reset the color of the last child in the sprite component
        Transform lastChild = spriteObject.GetChild(spriteObject.childCount - 1);
        if (lastChild != null)
        {
            Image lastChildRenderer = lastChild.GetComponent<Image>();
            if (lastChildRenderer != null)
            {
                lastChildRenderer.color = originalLastChildColor;
            }
        }
    }

    public void onClick()
    {
        // Spawn UIshot prefab at the cursor position
        Vector3 mousePosition = Input.mousePosition;
        Instantiate(uiShotPrefab, mousePosition, Quaternion.identity, transform);

        Vector3 forceDirection = (mousePosition - transform.position).normalized;

        // Add a Rigidbody component if not already present
        Rigidbody rb = gameObject.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.useGravity = true;
        }

        float forceMagnitude = 10f; // Adjust the force strength as needed
        Vector3 force = forceDirection * forceMagnitude;

        // Apply the force in the direction calculated
        rb.AddForce(force * 15, ForceMode.Impulse);

        // Add a force in the negative y-direction
        Vector3 negativeYForce = new Vector3(0, -forceMagnitude * 100, 0);
        rb.AddForce(negativeYForce, ForceMode.Impulse);

        // Apply torque
        rb.AddTorque(Random.insideUnitSphere * forceMagnitude, ForceMode.Impulse);

        // Set mass
        rb.mass = 100.0f;
    }
}
