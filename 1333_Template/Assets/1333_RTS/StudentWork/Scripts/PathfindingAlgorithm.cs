using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PathfindingAlgorithm : MonoBehaviour
{
    public abstract List<GridNode> FindPath(GridNode start, GridNode end);
    public abstract List<GridNode> FindPath(Vector3 start, Vector3 end);
}