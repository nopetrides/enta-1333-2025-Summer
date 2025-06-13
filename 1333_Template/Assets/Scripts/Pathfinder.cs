using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Pathfinder : MonoBehaviour
{

    [SerializeField] private GameObject playerUnits;
    [SerializeField] private GameObject EndUnit;
    [SerializeField] private GridManager gridManager;

    private List<GridNode> nodes = new List<GridNode>();

    GridNode spawnNode;
    GridNode EndNode;


    public void Start()
    {

        SpawnPlayerUnit();
        PathCheck();
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

                //StopDrawing = true;
            }
        }



    }
    private void DrawingLine(GridNode startGrid, GridNode endGrid)
    {

        GameObject drawingLine = new GameObject("Line");
        LineRenderer LineRend = drawingLine.AddComponent<LineRenderer>();

        LineRend.SetPosition(0, startGrid.WorldPosition);
        LineRend.SetPosition(1, endGrid.WorldPosition);

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
