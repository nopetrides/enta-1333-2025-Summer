using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Pathfinder : MonoBehaviour
{

    [SerializeField] private GameObject playerUnits;
    [SerializeField] private GameObject EndUnit;
    [SerializeField] private GridManager gridManager;

    private List<GridNode> nodes = new List<GridNode>();
    private List<GridNode> connectedNodes = new List<GridNode>();

    GridNode spawnNode;
    GridNode EndNode;

    private LineRenderer lineRenderer;
    public void Start()
    {

        SpawnPlayerUnit();

        PathCheck();
        AStarPathFinding();
    }
    private void  Remove()
    {




    }

    public void SpawnPlayerUnit()
    {
        if (gridManager == null || !gridManager.IsInitialized)
        {
            Debug.LogError("Spawner: GridManager not initialized.");
            return;
        }
        spawnNode = GetRandomWalkableNode();
        EndNode = GetRandomWalkableNode();
        if (EndNode == spawnNode)
        {
            GetRandomWalkableNode();
        }
        if (spawnNode == null)
        {
            Debug.LogError("Spawner: No walkable node found.");
            return;
        }
        if (playerUnits == null)
        {
            Debug.LogError("Spawner: playerUnits prefab is missing.");
            return;
        }
        Instantiate(playerUnits, spawnNode.WorldPosition, Quaternion.identity);
        Instantiate(EndUnit, EndNode.WorldPosition, Quaternion.identity);

        Debug.Log("Spawner: Spawned player unit at " + spawnNode.WorldPosition);

        if (GameObject.FindGameObjectWithTag("Target_Enemy"))
        {
            Debug.Log("Spawner: Target found.");
        }
    }

    private GridNode GetRandomWalkableNode()
    {
        List<GridNode> nodes = gridManager.GetAllNodes();
        int safety = 0;

        while (safety < 100)
        {
            GridNode node = nodes[Random.Range(0, nodes.Count)];
            if (node.Walkable)
                return node;

            safety++;
        }
        return null;
    }
    private void PathFinding()
    {
        nodes.Add(gridManager.gridNodes[0, 0]);
        //nodes.Add(gridManager.gridNodes[0, 0]);
        

    }
    private void PathCheck()
    {
       

        bool FoundStartNode = false;

        bool FoundEndNode = false;

        //bool StopDrawing = false;

        nodes = gridManager.GetAllNodes();
        
        foreach (GridNode node in nodes)
        {
            if (node.cords == spawnNode.cords)
            {
                //StopDrawing = false;
                //Gizmos.color = Color.red;
                FoundStartNode = true;
                transform.position = spawnNode.WorldPosition;
                Debug.Log("SpawnNode Found");
                
            }
            if (node.cords == EndNode.cords)
            {
                FoundEndNode = true;
                Debug.Log("EndNode Found");

            }
            if (FoundStartNode && FoundEndNode)
            {
                DrawingLine(spawnNode, EndNode);

                
            }
        }



    }
    private void AStarPathFinding()
    {




        bool foundStartNode = false;
        bool foundEndNode = false;

        // Get all nodes from the grid
        List<GridNode> allNodes = gridManager.GetAllNodes();

        foreach (GridNode node in allNodes)
        {
            if (node.cords == spawnNode.cords)
            {
                foundStartNode = true;
                Debug.Log("SpawnNode Found");
            }

            if (foundStartNode && !foundEndNode)
            {
                // Add each node after start node until we hit the end node
                connectedNodes.Add(node);
            }

            if (node.cords == EndNode.cords)
            {
                foundEndNode = true;
                Debug.Log("EndNode Found");

                if (!connectedNodes.Contains(node))
                {
                    connectedNodes.Add(node);
                }

                break;  // Optional: stop the loop if we reached end node
            }
        }

        // Check if we successfully collected the path
        if (foundStartNode && foundEndNode && connectedNodes.Count > 0)
        {
           DrawLine(connectedNodes);
        }
        else
        {
            Debug.LogWarning("Could not build a connected node path.");
        }



    }
    private void DrawLine(List<GridNode> connectedNodes)
    {
        /*
        if (connectedNodes == null || connectedNodes.Count == 0)
        {
            Debug.LogWarning("No nodes to draw line.");
            return;
        }
        */
        // Create one line object outside the loop
        GameObject lineObject = new GameObject("Algorithm Line");
        LineRenderer localLineRenderer = lineObject.AddComponent<LineRenderer>();

        localLineRenderer.positionCount = connectedNodes.Count;

        for (int i = 0; i < connectedNodes.Count; i++)
        {
            localLineRenderer.SetPosition(i, connectedNodes[i].WorldPosition);
        }

       
        localLineRenderer.widthMultiplier = 0.1f;
        localLineRenderer.startColor = Color.green;
        localLineRenderer.endColor = Color.green;

        
    }

    private void DrawingLine(GridNode startGrid, GridNode endGrid)
    {

        //GameObject drawingLine = new GameObject("Line");
        //LineRenderer LineRend = drawingLine.AddComponent<LineRenderer>();

        if (lineRenderer == null)
        {
            GameObject lineObj = new GameObject("PathLine");
            lineRenderer = lineObj.AddComponent<LineRenderer>();
            lineRenderer.positionCount = 2;
            lineRenderer.startWidth = 0.1f;
            lineRenderer.endWidth = 0.1f;

            
            //lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startColor = Color.green;
            lineRenderer.endColor = Color.red;
        }

        lineRenderer.SetPosition(0, startGrid.WorldPosition);
        lineRenderer.SetPosition(1, endGrid.WorldPosition);

        /*
        lineRenderer.positionCount = path.Count;

        for (int i = 0; i < path.Count; i++)
        {
            lineRenderer.SetPosition(i, path[i].WorldPosition);
        }
        */
    }



    private void OnDrawGizmos()
    {



        //if (EndNode.WorldPosition != null  && spawnNode.WorldPosition != null)
        //{
        //Gizmos.color = Color.red;


        //Gizmos.DrawLine(spawnNode.WorldPosition, EndNode.WorldPosition);

        //}
    }
}
