using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCController : MonoBehaviour
{
    private PlayerVision vision;

    // Start is called before the first frame update
    void Start()
    {
        vision = GetComponent<PlayerVision>();
    }

}
