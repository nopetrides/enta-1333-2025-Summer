using System.Collections.Generic;
using UnityEngine;

public class NaivePathfinder
{
    private readonly System.Func<Vector2Int, List<Vector2Int>> getNeighbors;
    private readonly bool allowDiagonal;
    private readonly int maxPathLength;

    public NaivePathfinder(
        System.Func<Vector2Int, List<Vector2Int>> getNeighbors,
        bool allowDiagonal = true,
        int maxPathLength = 100)
    {
        this.getNeighbors = getNeighbors;
        this.allowDiagonal = allowDiagonal;
        this.maxPathLength = maxPathLength;
    }

    public (List<Vector2Int> path, HashSet<Vector2Int> visited) FindPath(Vector2Int start, Vector2Int end)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        Vector2Int current = start;
        path.Add(current);
        visited.Add(current);

        while (current != end)
        {
            Vector2Int next;
            if (allowDiagonal)
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
                return (new List<Vector2Int>(), visited);
            }

            current = next;
            path.Add(current);
            visited.Add(current);

            if (path.Count > maxPathLength)
            {
                Debug.LogWarning("Naive pathfinding exceeded maximum path length!");
                return (new List<Vector2Int>(), visited);
            }
        }

        return (path, visited);
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
        return getNeighbors(pos).Count > 0;
    }
}