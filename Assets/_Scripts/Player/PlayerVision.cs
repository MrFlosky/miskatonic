using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum Vision { Up, Down, Left, Right }
public class PlayerVision : MonoBehaviour
{
    public Vision facing = Vision.Up;
    public Vector2Int gridPos;
    public int visionRange = 3;

    private List<Vector2Int> currentVisible = new List<Vector2Int>();

    public List<Vector2Int> GetVisibleCells()
    { 
        List<Vector2Int> visible  = new List<Vector2Int>();

        for (int i = 1; i < visionRange; i++)
        {
            switch (facing)
            {
                case Vision.Up:
                    visible.Add(new Vector2Int(gridPos.x, gridPos.y + i));
                    break;
                case Vision.Down:
                    visible.Add(new Vector2Int(gridPos.x, gridPos.y -  i));
                    break;
                case Vision.Left:
                    visible.Add(new Vector2Int(gridPos.x - i, gridPos.y));
                    break;
                case Vision.Right:
                    visible.Add(new Vector2Int(gridPos.x + i, gridPos.y));
                    break;
            }
        }

        return visible;
    }



}
