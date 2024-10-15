using TheKiwiCoder;
using UnityEngine;
using System.Collections.Generic;

public class IsStuck : ActionNode {
    // Store the last few positions to detect oscillation
    private Queue<Vector2Int> lastPositions = new Queue<Vector2Int>();
    public int maxTrackedPositions = 3;  // Number of positions to track to detect oscillation
    public float stuckThreshold = 0.1f;  // Time or distance threshold to consider AI as stuck

    // Track AI's current movement behavior
    protected override void OnStart() {
    }

    protected override void OnStop() {
        lastPositions.Clear();  // Clear positions if the node stops
    }

    protected override State OnUpdate() {
        Vector2Int currentPosition = context.agentNavigation.WorldToGridPosition(context.agentNavigation.transform.position);

        // Track the current position in the queue
        if (lastPositions.Count >= maxTrackedPositions) {
            lastPositions.Dequeue();  // Remove the oldest position if queue is full
        }
        lastPositions.Enqueue(currentPosition);

        // Check if AI is oscillating between a small set of positions
        if (IsOscillating()) {
            Debug.Log("AI is stuck, triggering backtrack.");
            return State.Success;  // AI is stuck, so trigger recovery/backtracking
        }

        return State.Failure;  // AI is not stuck, continue normal behavior
    }

    // Check if the AI is oscillating between the same positions
    private bool IsOscillating() {
        // Compare the positions to see if AI is stuck between a few positions
        Vector2Int[] positions = lastPositions.ToArray();
        for (int i = 0; i < positions.Length - 1; i++) {
            if (positions[i] != positions[i + 1]) {
                return false;  // AI has moved to a new position, not oscillating
            }
        }
        return true;  // AI is oscillating between the same positions, so it's stuck
    }
}