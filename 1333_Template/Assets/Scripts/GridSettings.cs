using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "GridSettings", menuName = "Game/GridSettings")]
public class GridSettings : ScriptableObject
{

    [SerializeField] private int gridSizeX = 10;

    [SerializeField] private int gridSizeY = 10;


    [SerializeField] private float nodeSize = 1f;

    [SerializeField] private bool useXZPlane;


    public int GridSizeX => gridSizeX;

    public int GridSizeY => gridSizeY;
    public float NodeSize => nodeSize;
    public bool UseXZPlane => useXZPlane;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
