using System.Collections.Generic;
using UnityEngine;

public class AStarTester : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private AStarPathfinder pathfinder;
    [SerializeField] private Transform startMarker;
    [SerializeField] private Transform endMarker;
    [SerializeField] private LineRenderer pathLine;

    [Header("Terrain Settings")]
    [SerializeField] private List<TerrainType> terrains;

    [Header("Debug Colors")]
    [SerializeField] private Color pathColor = Color.yellow;
    [SerializeField] private Color visitedColor = Color.blue;
    [SerializeField] private Color frontierColor = Color.cyan;

    private List<GridNode> lastPath;

    private void Start()
    {
        RandomizeGridTerrains();
        RunAStar();
    }

    public void RandomizeGridTerrains()
    {
        if (!gridManager.IsInitialized)
            gridManager.InitializeGrid();

        for (int x = 0; x < gridManager.GridSettings.GridSizeX; x++)
        {
            for (int y = 0; y < gridManager.GridSettings.GridSizeY; y++)
            {
                TerrainType type = terrains[Random.Range(0, terrains.Count)];
                GridNode node = gridManager.GetNode(x, y);
                node.Walkable = type.IsWalkable;
                node.Weight = type.MovementCost;
                node.TerrainType = type;
                gridManager.SetNode(x, y, node);
            }
        }
    }

    public void RunAStar()
    {
        lastPath = pathfinder.FindPath(startMarker.position, endMarker.position);
        DrawPathLine();
    }

    private void DrawPathLine()
    {
        if (pathLine == null || lastPath == null || lastPath.Count == 0)
        {
            if (pathLine != null) pathLine.positionCount = 0;
            return;
        }

        pathLine.positionCount = lastPath.Count;
        for (int i = 0; i < lastPath.Count; i++)
        {
            pathLine.SetPosition(i, lastPath[i].WorldPosition + Vector3.up * 0.05f);
        }
        pathLine.startColor = pathColor;
        pathLine.endColor = pathColor;
    }

    private void OnDrawGizmos()
    {
        if (gridManager == null || pathfinder == null) return;

        // Draw visited nodes
        Gizmos.color = visitedColor;
        foreach (var pair in pathfinder.VisitedNodes)
        {
            if (!pair.Value) continue;
            var node = gridManager.GetNode(pair.Key.x, pair.Key.y);
            Gizmos.DrawCube(node.WorldPosition, Vector3.one * gridManager.GridSettings.NodeSize * 0.4f);
        }

        // Draw frontier nodes
        Gizmos.color = frontierColor;
        foreach (var pair in pathfinder.FrontierNodes)
        {
            if (!pair.Value) continue;
            var node = gridManager.GetNode(pair.Key.x, pair.Key.y);
            Gizmos.DrawCube(node.WorldPosition, Vector3.one * gridManager.GridSettings.NodeSize * 0.6f);
        }

        // Draw the final path
        if (lastPath != null && lastPath.Count > 1)
        {
            Gizmos.color = pathColor;
            for (int i = 0; i < lastPath.Count - 1; i++)
                Gizmos.DrawLine(lastPath[i].WorldPosition, lastPath[i + 1].WorldPosition);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            RandomizeGridTerrains();
            RunAStar();
        }
    }
}
