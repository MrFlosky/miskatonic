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

    public string roomType = "Hallway";
    public void Init(bool top, bool bottom, bool right, bool left, bool roomTile, string roomType)
    {
        // set all walls as active dependent on bools given when created
        topWall.SetActive(top);
        bottomWall.SetActive(bottom);
        rightWall.SetActive(right);
        leftWall.SetActive(left);
        
        // if this tile is in the room, change meshes accordingly.
        if (!roomTile) return;
        
        this.roomType = roomType;
        
        floor.GetComponent<MeshRenderer>().material = roomFloorMaterial;
        topWall.GetComponent<MeshRenderer>().material = roomWallMaterial;
        bottomWall.GetComponent<MeshRenderer>().material = roomWallMaterial;
        rightWall.GetComponent<MeshRenderer>().material = roomWallMaterial;
        leftWall.GetComponent<MeshRenderer>().material = roomWallMaterial;
    }
}
