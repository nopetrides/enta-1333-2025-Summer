using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NaivePathVisualizer
{
    private readonly NaivePathfinder pathfinder;
    private readonly GridManager gridManager;
    private readonly System.Func<IEnumerator> waitForNextStep;
    private readonly System.Func<IEnumerator, Coroutine> startCoroutine;

    public NaivePathVisualizer(
        NaivePathfinder pathfinder,
        GridManager gridManager,
        System.Func<IEnumerator> waitForNextStep,
        System.Func<IEnumerator, Coroutine> startCoroutine)
    {
        this.pathfinder = pathfinder;
        this.gridManager = gridManager;
        this.waitForNextStep = waitForNextStep;
        this.startCoroutine = startCoroutine;
    }

    public IEnumerator VisualizePath(
        Vector2Int start,
        Vector2Int end,
        HashSet<Vector2Int> visitedNodes,
        List<Vector2Int> currentPath,
        System.Action<List<Vector3>> onPathFound)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        Vector2Int current = start;
        path.Add(current);
        visitedNodes.Add(current);
        currentPath.Add(current);

        while (current != end)
        {
            Vector2Int next;
            if (gridManager.GridSettings.AllowDiagonal)
            {
                next = GetNextNaivePosition(current, end);
            }
            else
            {
                next = GetNextCardinalNaivePosition(current, end);
            }

            if (next == current)
            {
                Debug.LogWarning("Naive pathfinding got stuck!");
                onPathFound(new List<Vector3>());
                yield break;
            }

            current = next;
            path.Add(current);
            visitedNodes.Add(current);
            currentPath.Add(current);

            if (path.Count > gridManager.GridSettings.GridSizeX * gridManager.GridSettings.GridSizeY)
            {
                Debug.LogWarning("Naive pathfinding exceeded maximum path length!");
                onPathFound(new List<Vector3>());
                yield break;
            }

            yield return waitForNextStep();
        }

        onPathFound(ConvertPathToWorldPositions(path));
    }

    private Vector2Int GetNextNaivePosition(Vector2Int current, Vector2Int target)
    {
        // Try to move directly towards target
        Vector2Int direction = new Vector2Int(
            Mathf.Clamp(target.x - current.x, -1, 1),
            Mathf.Clamp(target.y - current.y, -1, 1)
        );

        Vector2Int next = current + direction;
        if (IsValidMove(next))
        {
            return next;
        }

        // If direct move failed, try cardinal directions
        if (direction.x != 0)
        {
            next = current + new Vector2Int(direction.x, 0);
            if (IsValidMove(next))
            {
                return next;
            }
        }

        if (direction.y != 0)
        {
            next = current + new Vector2Int(0, direction.y);
            if (IsValidMove(next))
            {
                return next;
            }
        }

        // If all moves failed, try the other cardinal direction
        if (direction.x != 0)
        {
            next = current + new Vector2Int(0, direction.y);
            if (IsValidMove(next))
            {
                return next;
            }
        }
        else if (direction.y != 0)
        {
            next = current + new Vector2Int(direction.x, 0);
            if (IsValidMove(next))
            {
                return next;
            }
        }

        return current; // No valid moves found
    }

    private Vector2Int GetNextCardinalNaivePosition(Vector2Int current, Vector2Int target)
    {
        // Calculate which direction has the larger difference
        int dx = Mathf.Abs(target.x - current.x);
        int dy = Mathf.Abs(target.y - current.y);

        // Try to move in the direction with the larger difference first
        if (dx > dy)
        {
            // Try horizontal movement
            int signX = target.x > current.x ? 1 : -1;
            Vector2Int next = current + new Vector2Int(signX, 0);
            if (IsValidMove(next))
            {
                return next;
            }

            // If horizontal failed, try vertical
            int signY = target.y > current.y ? 1 : -1;
            next = current + new Vector2Int(0, signY);
            if (IsValidMove(next))
            {
                return next;
            }
        }
        else
        {
            // Try vertical movement
            int signY = target.y > current.y ? 1 : -1;
            Vector2Int next = current + new Vector2Int(0, signY);
            if (IsValidMove(next))
            {
                return next;
            }

            // If vertical failed, try horizontal
            int signX = target.x > current.x ? 1 : -1;
            next = current + new Vector2Int(signX, 0);
            if (IsValidMove(next))
            {
                return next;
            }
        }

        return current; // No valid moves found
    }

    private bool IsValidMove(Vector2Int pos)
    {
        return IsValidCoordinate(pos) && gridManager.IsWalkable(pos);
    }

    private bool IsValidCoordinate(Vector2Int coord)
    {
        return coord.x >= 0 && coord.x < gridManager.GridSettings.GridSizeX &&
               coord.y >= 0 && coord.y < gridManager.GridSettings.GridSizeY;
    }

    private List<Vector3> ConvertPathToWorldPositions(List<Vector2Int> path)
    {
        List<Vector3> worldPath = new List<Vector3>();
        foreach (Vector2Int coord in path)
        {
            worldPath.Add(GridToWorld(coord));
        }
        return worldPath;
    }

    private Vector3 GridToWorld(Vector2Int gridPos)
    {
        float nodeSize = gridManager.GridSettings.NodeSize;
        return new Vector3(gridPos.x * nodeSize, 0, gridPos.y * nodeSize);
    }
} 