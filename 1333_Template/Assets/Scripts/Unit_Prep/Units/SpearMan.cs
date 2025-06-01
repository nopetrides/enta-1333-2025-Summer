using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A concrete melee unit (SpearMan) that inherits from UnitBase.
/// Uses AStarPathfinder.FindPathWithNodes to obtain a List<Vector2Int> directly.
/// </summary>
public class SpearMan : UnitBase
{
    /// <summary>
    /// Moves this SpearMan to the specified target node.
    /// Uses AStarPathfinder.FindPathWithNodes to compute a List<Vector2Int> path.
    /// </summary>
    /// <param name="targetNode">The destination node on the grid.</param>
    public override void MoveTo(GridNode targetNode)
    {
        // Ensure GridManager and AStarPathfinder are available
        if (_gridManager == null || _pathfinder == null)
        {
            Debug.LogWarning($"SpearMan.MoveTo: Missing GridManager or AStarPathfinder on {name}.");
            return;
        }

        // 1) Determine the current grid node based on world position
        GridNode currentNode = _gridManager.getNodeFromWorldPosition(transform.position);
        if (currentNode.Equals(default(GridNode)))
        {
            Debug.LogWarning($"SpearMan.MoveTo: Could not identify current GridNode for {name}.");
            return;
        }

        // 2) Use AStarPathfinder.FindPathWithNodes to get a List<Vector2Int>
        List<Vector2Int> path = _pathfinder.FindPathWithNodes(
            currentNode,
            targetNode,
            Width,
            Height
        );

        // 3) If no path is found, log and return
        if (path == null || path.Count == 0)
        {
            Debug.Log($"SpearMan.MoveTo: No path found for {name} from {currentNode.name} to {targetNode.name}.");
            return;
        }

        // 4) Assign the new path and set state to Moving
        _currentPath = path;
        _nextPathIndex = 0;
        _state = UnitState.Moving;
    }

    /// <summary>
    /// Called automatically when the unit arrives at the final node in _currentPath.
    /// </summary>
    protected override void OnArrivedAtDestination()
    {
        base.OnArrivedAtDestination();
        // Custom behavior on arrival can go here (e.g., play idle animation)
    }
}
