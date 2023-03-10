using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Mirror;

public class BulletTrail : MonoBehaviour
{

    private Vector3 startPos;
    private Vector3 endPos;
    private float progess;

    [SerializeField] private float speed = 40f;

    void Start()
    {
        startPos = transform.position.WithAxis(Axis.Z, -1);
    }

    // Update is called once per frame
    void Update()
    {
        progess += Time.deltaTime * speed;
        transform.position = Vector3.Lerp(startPos, endPos, progess);
    }


    public void SetTargetPosition(Vector3 targetPos)
    {
        endPos = targetPos.WithAxis(Axis.Z, -1);
    }
}
