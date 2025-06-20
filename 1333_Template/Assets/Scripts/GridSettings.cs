using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GridSettings", menuName = "Game/GridSettings")]
public class GridSettings : ScriptableObject
{
    //  Grid settings
    [SerializeField] private int gridSizeX = 10;     // Number of nodes along X axis
    [SerializeField] private int gridSizeY = 10;     // Number of nodes along Y axis
    [SerializeField] private float nodeSize = 1f;    // Size of each grid node
    [SerializeField] private bool useXZPlane = true; // Toggle between XZ and XY plane

  
    public int GridSizeX => gridSizeX;
    public int GridSizeY => gridSizeY;
    public float NodeSize => nodeSize;
    public bool UseXZPlane => useXZPlane;
}