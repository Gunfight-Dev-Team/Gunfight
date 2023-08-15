using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CooldownVisualizer : MonoBehaviour
{
    public Image fill;
    public GameObject LocalPlayerObject;
    private PlayerController movementController;

    // Start is called before the first frame update
    void Start()
    {
        LocalPlayerObject = GameObject.Find("LocalGamePlayer");
        movementController =
            LocalPlayerObject.GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 pos = Input.mousePosition;
        pos.y -= Screen.height / 50.0f;
        pos.x -= Screen.width / 110.0f;
        transform.position = pos;
        if (movementController.cooldownTimer > 0)
        {
            fill.enabled = true;
            //Debug.Log(Mathf.Lerp(0, 1, movementController.cooldownTimer / movementController.PlayerInfo.cooldown));
            GetComponent<Slider>().value = Mathf.Lerp(0, 1, movementController.cooldownTimer / movementController.weaponInfo.cooldown);
        }
        else
        {
            fill.enabled = false;
        }
    }
}
