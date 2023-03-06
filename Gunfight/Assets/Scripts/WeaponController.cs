using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyPlayer
{
    public class WeaponController : MonoBehaviour
    {
        public WeaponInfo weapon;

        // Start is called before the first frame update
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
        }

        void OnTriggerStay2D(Collider2D other)
        {
            if (other.gameObject.name == "Player" && Input.GetKeyDown(KeyCode.E))
            {
                // [x] TODO: destroy the weapon
                // [x] TODO: pass the data to the local player
                Destroy(gameObject);
                other.GetComponent<PlayerInfo>().nAmmo = weapon.nAmmo;
                Debug.Log("Ammo: " + other.GetComponent<PlayerInfo>());
            }
        }
    }
}
