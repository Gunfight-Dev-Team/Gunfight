using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance;

    public Transform[] FFASpawnPoints;
    public Transform[] SPSpawnPoints;

    private void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
}
