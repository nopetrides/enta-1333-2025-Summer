using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GridManager))]
public class PathfindingManager : MonoBehaviour
{
    [Header("Pathfinding Settings")]
    // Grid coordinate for the starting position
    public Vector2Int startCoordinates;
    // Grid coordinate for the goal position
    public Vector2Int goalCoordinates;
    // Toggle to show or hide the path visualization
    public bool showPath = true;

    // Stores the currently calculated path as a list of grid coordinates
    private List<Vector2Int> path = new List<Vector2Int>();
    // Reference to the GridManager component
    private GridManager gridManager;
    // Reference to the A* pathfinder instance
    private AStarPathfinder astar;

    // Flag to enable or disable drawing Gizmos at runtime
    private bool drawGizmos = true;

    private void Awake()
    {
        // Cache the GridManager component
        gridManager = GetComponent<GridManager>();
        // Create a new A* pathfinder using the grid manager
        astar = new AStarPathfinder(gridManager);
        // Calculate initial path
        RecalculatePath();
    }

    private void OnValidate()
    {
        // Ensure gridManager is assigned, especially when changing values in the Inspector
        if (gridManager == null)
            gridManager = GetComponent<GridManager>();

        // If the grid is not initialized yet, do not attempt to recalculate the path
        if (gridManager == null || !gridManager.isInitialized)
            return;

        // If the A* pathfinder instance is null (e.g., after scripts recompile), recreate it
        if (astar == null)
            astar = new AStarPathfinder(gridManager);

        // Recalculate the path whenever Inspector values change
        RecalculatePath();
    }

    private void Start()
    {
        // Ensure the path is recalculated at the start of the game
        RecalculatePath();
    }

    private void Update()
    {
        // Toggle the Gizmo drawing on/off when the player presses the G key
        if (Input.GetKeyDown(KeyCode.G))
        {
            drawGizmos = !drawGizmos;
        }
    }

    /// <summary>
    /// Calculates the path from startCoordinates to goalCoordinates using A*.
    /// </summary>
    private void RecalculatePath()
    {
        // If showPath is disabled or the grid is not ready, clear any existing path
        if (!showPath || gridManager == null || !gridManager.isInitialized)
        {
            path.Clear();
            return;
        }

        // Use A* algorithm to find a new path
        path = astar.FindPath(startCoordinates, goalCoordinates);
    }

    /// <summary>
    /// Public callback that can be invoked when the grid data changes (e.g., obstacles update).
    /// Ensures the path is recalculated to reflect the new grid state.
    /// </summary>
    public void GridUpdated()
    {
        RecalculatePath();
    }

    private void OnDrawGizmos()
    {
        // Conditions to skip drawing Gizmos:
        // - showPath is false
        // - drawGizmos is false (user toggled off)
        // - path has not been calculated
        // - gridManager is not assigned or not initialized
        if (!showPath || !drawGizmos || path == null || gridManager == null || !gridManager.isInitialized)
            return;

        // Calculate a scaled size for the cubes/spheres based on node size
        float size = gridManager.GridSettings.NodeSize * 0.3f;

        // Draw each node in the path as a red cube, and connect consecutive nodes with lines
        Gizmos.color = Color.red;
        for (int i = 0; i < path.Count; i++)
        {
            Vector2Int coord = path[i];
            GridNode node = gridManager.GetNode(coord.x, coord.y);
            // Slightly raise the wireframe above the grid to avoid Z-fighting
            Vector3 worldPos = node.worldPosition + Vector3.up * 0.1f;
            Gizmos.DrawCube(worldPos, Vector3.one * size);

            // Draw a connecting line from the previous node to the current node
            if (i > 0)
            {
                Vector2Int prev = path[i - 1];
                GridNode prevNode = gridManager.GetNode(prev.x, prev.y);
                Vector3 prevWorldPos = prevNode.worldPosition + Vector3.up * 0.1f;
                Gizmos.DrawLine(prevWorldPos, worldPos);
            }
        }

        // Highlight the start node with a larger magenta cube
        if (path.Count > 0)
        {
            Gizmos.color = Color.magenta;
            GridNode startNode = gridManager.GetNode(startCoordinates.x, startCoordinates.y);
            Vector3 startPos = startNode.worldPosition + Vector3.up * 0.2f;
            Gizmos.DrawCube(startPos, Vector3.one * size * 1.2f);
        }

        // Highlight the end node with a larger blue cube
        if (path.Count > 0)
        {
            Gizmos.color = Color.blue;
            GridNode endNode = gridManager.GetNode(goalCoordinates.x, goalCoordinates.y);
            Vector3 endPos = endNode.worldPosition + Vector3.up * 0.2f;
            Gizmos.DrawCube(endPos, Vector3.one * size * 1.2f);
        }
    }
}
