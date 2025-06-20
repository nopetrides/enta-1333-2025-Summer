using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualTargetPath : MonoBehaviour
{
    
    [SerializeField] private GridManager gridManager;                 // Reference to the grid manager
    [SerializeField] private AStartPathfinding pathfindingLogic;      // Reference to the A* pathfinding system
    [SerializeField] private GameObject startPosPrefab;               // Prefab for the start marker
    [SerializeField] private GameObject endPosPrefab;                 // Prefab for the end marker
    [SerializeField] private GameObject movingAgent;                 // The agent that will move along the path
    [SerializeField] private float searchDelay = 0.1f;               // Delay between pathfinding steps (for visual effect)

    
    private LineRenderer lineRenderer;                               // LineRenderer to draw the path
    private List<GridNode> pathNodes = new();                        // Calculated path nodes
    private GameObject startInstance;                                // Spawned start marker instance
    private GameObject endInstance;                                  // Spawned end marker instance
    private GridNode startNode;                                      // Selected start node
    private GridNode endNode;                                        // Selected end node

  
    private void Awake()
    {
        SetupLineRenderer();                                         // Initialize the LineRenderer component
    }

   
    public void ResetFeild()
    {
        // Clean up old start and end markers
        if (startInstance != null) Destroy(startInstance);
        if (endInstance != null) Destroy(endInstance);

        // Clear the drawn path
        lineRenderer.positionCount = 0;

        // Remove any direct line objects from the scene
        foreach (var line in GameObject.FindGameObjectsWithTag("Untagged"))
        {
            if (line.name.Contains("DirectLine"))
                Destroy(line);
        }

        // Start generating a new path
        StartCoroutine(GeneratePath());
    }

   
    private IEnumerator GeneratePath()
    {
        List<GridNode> allNodes = gridManager.GetAllNodes();
        if (allNodes == null || allNodes.Count < 2) yield break;

        // Pick random walkable start and end nodes (ensuring they are not the same)
        startNode = GetRandomWalkableNode();
        endNode = GetRandomWalkableNode();
        while (endNode == startNode)
            endNode = GetRandomWalkableNode();

        // Instantiate visual markers
        startInstance = Instantiate(startPosPrefab, startNode.WorldPosition, Quaternion.identity);
        endInstance = Instantiate(endPosPrefab, endNode.WorldPosition, Quaternion.identity);

        Debug.Log($"Start: {startNode.Name}, End: {endNode.Name}");

        // Find the path using the pathfinding system
        pathNodes = FindPath(startNode, endNode);

        // Delay loop for visual effect (optional)
        foreach (GridNode node in pathNodes)
        {
            yield return new WaitForSeconds(searchDelay);
        }

        if (pathNodes.Count > 0)
        {
            DrawPath(pathNodes);

            // If the moving agent has a Player_Targeting script, command it to start moving
            if (movingAgent.TryGetComponent(out Player_Targeting mover))
            {
                mover.StartMoving(pathNodes);
            }
        }
        else
        {
            Debug.Log("Path could not be found.");
            ResetFeild(); // Retry if path fails
        }
    }

   

    // Wrapper to call the pathfinding system
    private List<GridNode> FindPath(GridNode startNode, GridNode endNode)
    {
        if (pathfindingLogic == null)
        {
            Debug.LogError("AStartPathfinding reference not set in inspector!");
            return new List<GridNode>();
        }
        Debug.Log("Using A* pathfinding");
        return pathfindingLogic.FindPath(gridManager, startNode, endNode, 1, 1);
    }

    // Randomly select a walkable node from the grid
    private GridNode GetRandomWalkableNode()
    {
        var nodes = gridManager.GetAllNodes();
        GridNode node;
        do node = nodes[Random.Range(0, nodes.Count)];
        while (!node.walkable);
        return node;
    }

    // Draw the calculated path using the LineRenderer
    private void DrawPath(List<GridNode> path)
    {
        if (lineRenderer == null) return;

        lineRenderer.positionCount = path.Count;
        for (int i = 0; i < path.Count; i++)
        {
            lineRenderer.SetPosition(i, path[i].WorldPosition + Vector3.up * 0.2f);  // Slight vertical offset for visibility
        }
    }

    // Set up the LineRenderer if not already present
    private void SetupLineRenderer()
    {
        if (lineRenderer == null)
            lineRenderer = gameObject.AddComponent<LineRenderer>();

        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.blue;
        lineRenderer.endColor = Color.black;
        lineRenderer.startWidth = 0.2f;
        lineRenderer.endWidth = 0.2f;
        lineRenderer.useWorldSpace = true;
        lineRenderer.positionCount = 0;
    }
}
