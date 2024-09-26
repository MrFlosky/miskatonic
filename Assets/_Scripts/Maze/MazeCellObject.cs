using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeCellObject : MonoBehaviour
{
    [SerializeField] GameObject topWall;
    [SerializeField] GameObject bottomWall;
    [SerializeField] GameObject rightWall;
    [SerializeField] GameObject leftWall;
    [SerializeField] GameObject floor;
    
    [SerializeField] Material roomFloorMaterial;
    [SerializeField] Material roomWallMaterial;
    public void Init(bool top, bool bottom, bool right, bool left, bool roomTile)
    {
        topWall.SetActive(top);
        bottomWall.SetActive(bottom);
        rightWall.SetActive(right);
        leftWall.SetActive(left);
        if (roomTile)
        {
            floor.GetComponent<MeshRenderer>().material = roomFloorMaterial;
            
            topWall.GetComponent<MeshRenderer>().material = roomWallMaterial;
            bottomWall.GetComponent<MeshRenderer>().material = roomWallMaterial;
            rightWall.GetComponent<MeshRenderer>().material = roomWallMaterial;
            leftWall.GetComponent<MeshRenderer>().material = roomWallMaterial;
        }

    }
}
