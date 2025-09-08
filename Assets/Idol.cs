using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Idol : MonoBehaviour
{

    public float range = 4f;


    public void Capture()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, range);
        foreach (var hitCollider in hitColliders)
        {
            Unconverted u = hitCollider.gameObject.GetComponent<Unconverted>();
            if (u != null)
            {
                u.Convert();
            }
        }
    }









    void Update()
    {
        if (!MinigameManager.Instance.gameRunning)
        {
            Destroy(this.gameObject);
        }
    }


}
