using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MazeRenderer))]
public class MazeRendererEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        MazeRenderer mazeRenderer = (MazeRenderer)target;
        if (GUILayout.Button("Generate Maze"))
        {
            mazeRenderer.GenerateMaze();
        }
    }
}
