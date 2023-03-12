using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WeaponSpawning : MonoBehaviour
{

    [SerializeField] private GameObject[] weapons;

    [SerializeField] private Tilemap tileMap;

    [SerializeField] private int numWeapons;

    // Start is called before the first frame update
    void Start()
    {
        List<Vector3> tileLoc = new List<Vector3>();
        foreach(var pos in tileMap.cellBounds.allPositionsWithin)
        {
            Vector3Int localPlace = new Vector3Int(pos.x, pos.y, pos.z);
            Vector3 place = tileMap.CellToWorld(localPlace);
            if (tileMap.HasTile(localPlace))
            {
                tileLoc.Add(place);
            }
        }

        foreach(GameObject weapon in weapons)
        {
            for (int i = 0; i < numWeapons; i++)
            {
                Instantiate(weapon, tileLoc[Random.Range(0, tileLoc.Count)], Quaternion.Euler(0, 0, Random.Range(0, 360)));
            }
        }
    }
}
