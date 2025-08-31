using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="GridSettings", menuName = "Game/GridSettings")]
public class GridSettings : ScriptableObject
{
    // Width of the grid (number of nodes in the X direction)
    [SerializeField] private int gridSizeX = 10;

    // Height of the grid (number of nodes in the Y direction)
    [SerializeField] private int gridSizeY = 10;

    // Size of each node (in world units)
    [SerializeField] private float nodeSize = 1f;

    // Determines if the grid should be on the XZ plane (true) or XY plane (false)
    [SerializeField] private bool useXZPlane = true;

    // Public read-only properties to access the private fields
    public int GridSizeX => gridSizeX;
    public int GridSizeY => gridSizeY;
    public float NodeSize => nodeSize;
    public bool UseXZPlane => useXZPlane;
}
