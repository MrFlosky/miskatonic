using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class MazeCellObject : MonoBehaviour
{
    private int xCoord;
    private int yCoord;
    // Totem check
    public bool hasTotem = false;

    [SerializeField] GameObject topWall;
    [SerializeField] GameObject bottomWall;
    [SerializeField] GameObject rightWall;
    [SerializeField] GameObject leftWall;
    [SerializeField] GameObject floor;
    
    [SerializeField] public Material roomFloorMaterial; // I set these to public to be accessed by room swap
    [SerializeField] public Material roomWallMaterial;

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

    /// <summary>
    /// Resets this cell objects material to any new set by other scripts.
    /// Used by swap rooms.
    /// </summary>
    public void RefreshMaterial()
    {
        floor.GetComponent<MeshRenderer>().material = roomFloorMaterial;
        topWall.GetComponent<MeshRenderer>().material = roomWallMaterial;
        bottomWall.GetComponent<MeshRenderer>().material = roomWallMaterial;
        rightWall.GetComponent<MeshRenderer>().material = roomWallMaterial;
        leftWall.GetComponent<MeshRenderer>().material = roomWallMaterial;
    }

    /// <summary>
    /// Getters and setters for the x,y coord associated with this cell.
    /// Used by skills and set ONLY upon initialization.
    /// </summary>
    public int GetXCoord()
    {
        return xCoord;
    }
    public int GetYCoord()
    {
        return yCoord;
    }
    public void SetXCoord(int x)
    {
        xCoord = x;
    }
    public void SetYCoord(int y)
    {
        yCoord = y;
    }
    
    public Vector2Int GetLocalCoordinates(int roomTopLeftX, int roomTopLeftY)
    {
        // Calculate the local x and y coordinates based on the room's top-left corner
        int localX = GetXCoord() - roomTopLeftX;
        int localY = GetYCoord() - roomTopLeftY;

        // Return the local coordinates as a Vector2Int
        return new Vector2Int(localX, localY);
    }
}
