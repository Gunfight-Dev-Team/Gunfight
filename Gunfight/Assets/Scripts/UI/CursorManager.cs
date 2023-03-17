using UnityEngine;
using UnityEngine.UI;

public class CursorManager : MonoBehaviour
{
    // Reference to the cursor image or sprite
    [SerializeField] private Texture2D cursorTexture;

    private void Start()
    {
        Cursor.SetCursor(cursorTexture, new Vector2(4, 4), CursorMode.Auto);
    }

    // Update is called once per frame
    void Update()
    {
        //float cooldownRatio = gun.GetCooldownRatio();
        //Color cursorColor = new Color(1f - cooldownRatio, 1f - cooldownRatio, 1f - cooldownRatio);
        //Cursor.SetCursor(cursorTexture, new Vector2(4, 4), CursorMode.ForceSoftware, 0, cursorColor);
    }
}
