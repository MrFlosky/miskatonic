using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum Vision { Up, Down, Left, Right }
public class PlayerVision : MonoBehaviour
{
    public Vision facing = Vision.Up;
    public Vector2Int gridPos;
    public int visionRange = 3;

	[SerializeField] Material outlineMaterial;  // Material applied when highlighting tiles

	//private List<Vector2Int> currentVisible = new List<Vector2Int>();

	private List<Vector2Int> currentVisible = new List<Vector2Int>();
	private MazeCellObject currentCell;
	private Dictionary<MeshRenderer, Material[]> originalMaterials
	= new Dictionary<MeshRenderer, Material[]>();

	void Start()
	{
		// Try to get the MazeCellObject this player is standing on
		currentCell = GetComponent<MazeCellObject>();

		if (currentCell != null)
		{
			gridPos = new Vector2Int(currentCell.GetXCoord(), currentCell.GetYCoord());
		}
		else
		{
			Debug.LogWarning("PlayerVision: No MazeCellObject attached. " +
							 "Make sure you assign currentCell when spawning!");
		}
	}

	void Update()
	{
		// If this character is tied to a MazeCellObject, always sync grid position
		if (currentCell != null)
		{
			gridPos = new Vector2Int(currentCell.GetXCoord(), currentCell.GetYCoord());
			
		}
		else
		{
			// fallback: find nearest cell from the maze
			gridPos = FindNearestCellGrid(transform.position);
			Debug.Log(gridPos.x);
			Debug.Log(gridPos.y);
		}

	}

	public List<Vector2Int> GetVisibleCells()
    { 
        List<Vector2Int> visible  = new List<Vector2Int>();

		for (int i = 1; i <= visionRange; i++)
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

	private Vector2Int FindNearestCellGrid(Vector3 worldPos)
	{
		float best = float.MaxValue;
		MazeCellObject nearest = null;

		foreach (var kv in MazeRenderer.Instance.cellGameObjects)
		{
			Vector3 cellWorld = kv.Value.transform.position;
			float d = (cellWorld - worldPos).sqrMagnitude;
			if (d < best)
			{
				best = d;
				nearest = kv.Value.GetComponent<MazeCellObject>();
			}
		}

		if (nearest != null)
			return new Vector2Int(nearest.GetXCoord(), nearest.GetYCoord());

		return Vector2Int.zero;
	}

	public void UpdateVision()
	{
		var agentNavigation = GetComponent<AgentNavigation>();
		var gameManager = agentNavigation.gameManager;

		// 1. Clear old highlights
		foreach (var pos in currentVisible)
		{
			if (MazeRenderer.Instance.cellGameObjects.TryGetValue(pos, out GameObject cellObject))
			{
				foreach (Transform child in cellObject.transform)
				{
					// Reset to original materials
					var renderer = child.GetComponent<MeshRenderer>();
					//if (renderer != null)
					//{
					//var mats = new List<Material>(renderer.materials);
					//mats.Remove(outlineMaterial);
					//renderer.materials = mats.ToArray();
					//}
					if (renderer != null && originalMaterials.TryGetValue(renderer, out var mats))
					{
						renderer.materials = mats; // restore baseline
					}
				}
			}
		}

		// 2. Compute new vision
		currentVisible = GetVisibleCells();

		// 3. Apply new highlights
		foreach (var pos in currentVisible)
		{
			if (MazeRenderer.Instance.cellGameObjects.TryGetValue(pos, out GameObject cellObject))
			{
				foreach (Transform child in cellObject.transform)
				{
					var renderer = child.GetComponent<MeshRenderer>();
					//if (renderer != null)
					//{
					//	var mats = new List<Material>(renderer.materials);
					//	if (!mats.Contains(outlineMaterial)) // avoid duplicates
					//		mats.Add(outlineMaterial);
					//	renderer.materials = mats.ToArray();
					//}
					if (renderer != null)
					{
						if (!originalMaterials.ContainsKey(renderer))
							originalMaterials[renderer] = renderer.materials;

						var mats = new List<Material>(renderer.materials);
						if (!mats.Contains(outlineMaterial))
							mats.Add(outlineMaterial);

						renderer.materials = mats.ToArray();
					}
				}
			}
		}
	}

}
