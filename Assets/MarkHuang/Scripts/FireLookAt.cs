using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireLookAt : MonoBehaviour
{
    private Vector3 cameraPos;

    // Start is called before the first frame update
    void Start()
    {
        cameraPos = GameObject.Find("Main Camera").transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 directionToTarget = cameraPos - transform.position;
        Vector3 backDirection = -directionToTarget.normalized;
        Quaternion targetRotation = Quaternion.LookRotation(backDirection);
        transform.rotation = targetRotation;
    }
}
