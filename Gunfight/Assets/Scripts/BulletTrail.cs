using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Mirror;

public class BulletTrail : NetworkBehaviour
{

    private Vector3 startPos;
    private Vector3 endPos;
    private float progess;

    [SerializeField] private float speed = 40f;

    public override void OnStartServer()
    {
        Invoke(nameof(DestroySelf), 0.02f);
    }

    void Start()
    {
        startPos = transform.position.WithAxis(Axis.Z, -1);
    }

    // destroy for everyone on the server
    [Server]
    void DestroySelf()
    {
        NetworkServer.Destroy(gameObject);
    }

    // ServerCallback because we don't want a warning
    // if OnTriggerEnter is called on the client
    [ServerCallback]
    void OnTriggerEnter(Collider co) => DestroySelf();

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
