using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MazeRenderer))]
public class MazeRendererEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // run the basic oninspector for MazeRenderer
        base.OnInspectorGUI();
        MazeRenderer mazeRenderer = (MazeRenderer)target;
        
        // add our own buttons
        
        // generate maze button
        if (GUILayout.Button("Generate Maze"))
        {
            mazeRenderer.GenerateMaze();
        }
        // destroy room button
        if (GUILayout.Button("Destroy Rooms"))
        {
            mazeRenderer.DestroyChildren();
        }
    }
}
