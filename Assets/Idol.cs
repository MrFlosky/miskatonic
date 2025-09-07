using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Idol : MonoBehaviour
{

    void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.gameObject.name);
        Unconverted u = other.gameObject.GetComponent<Unconverted>();

        if (u != null)
        {
            u.Convert();
            ResourceManager.Instance.AddWorship(1);
        }

    }

    void Update()
    {
        if (MinigameManager.Instance.ending == true)
        {
            Destroy(this.gameObject);
        }
    }


}
