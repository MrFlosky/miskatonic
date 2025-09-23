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

    // Update is called once per frame
    void Update()
    {
        vision.gridPos = new Vector2Int(Mathf.RoundToInt(transform.position.x),
                                        Mathf.RoundToInt(transform.position.y));

        List<Vector2Int> cells = vision.GetVisibleCells();

        foreach (var c in cells)
        {
            Debug.Log(gameObject.name + " sees " + c);
        }

    }
}
