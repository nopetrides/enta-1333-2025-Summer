using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] private GameObject playerUnits;
    [SerializeField] private GameObject EndUnit;
    [SerializeField] private GridManager gridManager;

    GridNode spawnNode;
    GridNode EndNode;

    
    public void Start()
    {

            SpawnPlayerUnit(); 
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
    private void OnDrawGizmos()
    {
     


        //if (EndNode.WorldPosition != null  && spawnNode.WorldPosition != null)
        //{
        //Gizmos.color = Color.red;


        //Gizmos.DrawLine(spawnNode.WorldPosition, EndNode.WorldPosition);

        //}
    }
}
