using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
[ExecuteInEditMode]
public class PathFinding : MonoBehaviour
{
    [SerializeField] private GridManager gridManager;

    private List<GridNode> finalPath = new();
    private GridNode[,] grid;

    private void Start()
    {
        Reset();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Reset();
        }
    }
    private void Reset()
    {
        gridManager.InitializedGrid();

        BuildGridReference(); //converts the node list into a 2D array for easier indexing.

        Vector2Int start = GetRandomCoord(); //picks two random grid coordinates.
        Vector2Int end = GetRandomCoord();

        finalPath = DepthFirstSearch(start, end); //stores the result to be drawn.
    }
    void BuildGridReference()
    {
        int width = gridManager.GridSettings.GridSizeX;
        int height = gridManager.GridSettings.GridSizeY;
        grid = new GridNode[width, height]; //Initializes a 2D array (grid) to store the nodes by position.

        foreach (var node in GetAllNodes())
        {
            string[] parts = node.Name.Split('_'); //Extracts x, y from the node’s name ("Type_3_7" x=3, y=7).
            int x = int.Parse(parts[1]);
            int y = int.Parse(parts[2]);
            grid[x, y] = node; //Fills the grid so you can access nodes as grid[x, y].
        }
    }

    List<GridNode> GetAllNodes()
    {
        var field = typeof(GridManager).GetField("AllNodes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance); //Uses reflection to access the
                                                                                                                                                  //private list AllNodes in GridManager.
                                                                                                                                                  //AllNodes is the raw list of all
                                                                                                                                                  //GridNode structs.
        return field.GetValue(gridManager) as List<GridNode>;
    }
    
    Vector2Int GetRandomCoord()
    {
        return new Vector2Int(
            Random.Range(0, gridManager.GridSettings.GridSizeX),
            Random.Range(0, gridManager.GridSettings.GridSizeY)
        );
    }

    List<GridNode> DepthFirstSearch(Vector2Int startCoord, Vector2Int endCoord)
    {
        Stack<GridNode> stack = new(); //stack: keeps track of which nodes to explore next (DFS = LIFO)
        Dictionary<GridNode, GridNode> cameFrom = new(); //cameFrom: to reconstruct the path once the end is found
        HashSet<GridNode> visited = new(); //visited: to avoid infinite loops

        GridNode start = grid[startCoord.x, startCoord.y];
        GridNode end = grid[endCoord.x, endCoord.y];

        stack.Push(start); ////Start the search with the starting node.
        visited.Add(start);

        while (stack.Count > 0)
        {
            /*Take the last node from the stack.
              If it's the end, stop and build the path.
              Otherwise, explore its neighbors.
              If neighbor is walkable and unvisited, add it to the stack and record the breadcrumb.*/

            GridNode current = stack.Pop();

            if (current.Equals(end))
            {
                return ReconstructPath(cameFrom, current);
            }

            List<GridNode> neighbors = GetNeighbors(current);

            // Sort neighbors by movement cost (cheapest last so it gets popped first)
            neighbors.Sort((a, b) => b.terrainType.MovementCost.CompareTo(a.terrainType.MovementCost));

            foreach (GridNode neighbor in neighbors)
            {
                if (!neighbor.walkable || visited.Contains(neighbor)) continue;

                visited.Add(neighbor);
                stack.Push(neighbor);
                cameFrom[neighbor] = current;
            }
        }

        return new List<GridNode>(); // No path found
    }

    //Backtracks from the end to the start using the cameFrom dictionary.
    //Inserts each node at the start of the list to keep order from start to the goal.
    List<GridNode> ReconstructPath(Dictionary<GridNode, GridNode> cameFrom, GridNode current)
    {
        List<GridNode> path = new() { current };
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Insert(0, current);
        }
        return path;
    }

    List<GridNode> GetNeighbors(GridNode node)
    {
        List<GridNode> neighbors = new();
        string[] split = node.Name.Split('_');
        int x = int.Parse(split[1]);
        int y = int.Parse(split[2]);

        Vector2Int[] directions = new Vector2Int[] //Checks four cardinal directions (up, right, down, left). Valid neighbors are added to the list.
        {
            new(0, 1), new(1, 0), new(0, -1), new(-1, 0)
        };

        foreach (Vector2Int dir in directions)
        {
            int nx = x + dir.x;
            int ny = y + dir.y;

            if (nx >= 0 && nx < grid.GetLength(0) &&
                ny >= 0 && ny < grid.GetLength(1))
            {
                neighbors.Add(grid[nx, ny]);
            }
        }

        return neighbors;
    }

    private void OnDrawGizmos() //Draws the path with spheres so it's easier to visualize
    {
        if (finalPath == null || finalPath.Count == 0) return;

        Gizmos.color = Color.cyan;

        for (int i = 0; i < finalPath.Count; i++)
        {
            Gizmos.DrawSphere(finalPath[i].WorldPosition, 0.3f); // Bigger radius = better visibility
        }

        for (int i = 0; i < finalPath.Count - 1; i++)
        {
            Gizmos.DrawLine(finalPath[i].WorldPosition, finalPath[i + 1].WorldPosition);
        }
    }
}
