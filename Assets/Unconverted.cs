using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls the 'Unconverted' NPCs in the idol minigame
/// </summary>
public class Unconverted : MonoBehaviour
{
    public float speed = 0;

    public bool hidden;
    public Material convertedMat;
    private bool converted = false;


    void FixedUpdate()
    {
        transform.position = transform.position + (transform.forward * speed);
    }

    public void Hide()
    {
        hidden = true;
        GetComponent<MeshRenderer>().enabled = false;
    }

    public void Show()
    {
        hidden = false;
        GetComponent<MeshRenderer>().enabled = true;
    }

    void Update()
    {
        if (!MinigameManager.Instance.gameRunning == true)
        {
            Destroy(this.gameObject);
        }
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawLine(Vector3.zero, Vector3.forward);
    }


    public void Convert()
    {
        if (converted) return;
        converted = true;
        speed = 0;
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        renderer.material = convertedMat;
        ResourceManager.Instance.AddWorship(1);
    }

}
