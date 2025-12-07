using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script is responsible for making portal cam follow main cam.
public class PortalCamFollow : MonoBehaviour
{
    Transform trans;
    Camera cam;
    // Start is called before the first frame update
    void Awake()
    {
        trans = GetComponent<Transform>();
        cam = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        trans.position = Camera.main.transform.position;
        trans.rotation = Camera.main.transform.rotation;
    }
}
