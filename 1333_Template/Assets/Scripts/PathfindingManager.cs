using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GridManager))]
public class PathfindingManager : MonoBehaviour
{
    public enum PathfinderType { AStar, BruteForce }

    [Header("Algorithm")]
    public PathfinderType algorithm = PathfinderType.AStar;

    [Header("Pathfinding Settings")]
    public Vector2Int startCoordinates;
    public Vector2Int goalCoordinates;
    public bool showPath = true;

    private List<Vector2Int> path = new List<Vector2Int>();
    private GridManager gridManager;
    private AStarPathfinder astar;      
    private BruteForcePathfinder brute;

    private void Awake()
    {
        gridManager = GetComponent<GridManager>();
        astar = new AStarPathfinder(gridManager);  
        brute = new BruteForcePathfinder(gridManager);
        RecalculatePath();
    }

    private void OnValidate()
    {
        if (gridManager == null)
            gridManager = GetComponent<GridManager>();
        if (gridManager == null || !gridManager.isInitialized)
            return;
        if (astar == null)
            astar = new AStarPathfinder(gridManager);
        if (brute == null)
            brute = new BruteForcePathfinder(gridManager);
        RecalculatePath();
    }

    private void Start()
    {
        RecalculatePath();
    }

    private void RecalculatePath()
    {
        if (!showPath || gridManager == null || !gridManager.isInitialized)
        {
            path.Clear();
            return;
        }

        switch (algorithm)
        {
            case PathfinderType.AStar:
                path = astar.FindPath(startCoordinates, goalCoordinates);
                break;
            case PathfinderType.BruteForce:
                path = brute.FindPath(startCoordinates, goalCoordinates);
                break;
        }
    }

    public void GridUpdated()
    {
        RecalculatePath();
    }

    private void OnDrawGizmos()
    {
        if (!showPath || path == null || gridManager == null || !gridManager.isInitialized)
            return;

        Gizmos.color = Color.red;
        float size = gridManager.GridSettings.NodeSize * 0.3f;

        for (int i = 0; i < path.Count; i++)
        {
            var coord = path[i];
            var node = gridManager.GetNode(coord.x, coord.y);
            Gizmos.DrawCube(node.worldPosition + Vector3.up * 0.1f, Vector3.one * size);

            if (i > 0)
            {
                var prev = path[i - 1];
                var prevNode = gridManager.GetNode(prev.x, prev.y);
                Gizmos.DrawLine(prevNode.worldPosition + Vector3.up * 0.1f,
                                node.worldPosition + Vector3.up * 0.1f);
            }
        }
    }
}