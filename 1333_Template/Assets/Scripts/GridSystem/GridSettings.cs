using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GridSettings", menuName = "Game/GridSettings")]
public class GridSettings : ScriptableObject
{
    [SerializeField] private int _gridSizeX = 10;
    [SerializeField] private int _gridSizeY = 10;
    [SerializeField] private float _nodeSize = 1f;
    [SerializeField] private bool _useXZPlane = true;

    public int GridSizeX => _gridSizeX;
    public int GridSizeY => _gridSizeY;
    public float NodeSize => _nodeSize;
    public bool UseXZPlane => _useXZPlane;
}
