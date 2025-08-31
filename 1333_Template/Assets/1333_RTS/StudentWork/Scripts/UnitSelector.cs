using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class UnitSelector : MonoBehaviour
{
    private Camera cam;

    [SerializeField] private UnitManager unitManager;     // Manages unit list and selection
    [SerializeField] private GridManager gridManager;     // Accesses grid data for pathfinding
    [SerializeField] private AStarPathfinding pathfinder; // Pathfinding system (not directly used here)
    [SerializeField] private LayerMask groundLayer;       // LayerMask to filter raycast to ground only

    private UnitInstance selectedUnit; // (Deprecated) single-unit selection reference

    void Start()
    {
        // Cache the main camera for raycasting
        cam = Camera.main;
    }

    void Update()
    {
        HandleSelection();         // Handles left-click unit selection
        HandleRightClickCommand(); // Handles right-click movement commands
    }

    // Handles selecting units via left mouse click
    void HandleSelection()
    {
        if (Input.GetMouseButtonDown(0)) // Left click
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // Check if the clicked object is a unit
                var unit = hit.collider.GetComponent<UnitInstance>();
                // If holding Shift, allow multi-selection
                bool additive = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

                if (unit != null)
                {
                    unitManager.SelectUnit(unit, additive); // Add to selected list
                }
                else if (!additive)
                {
                    unitManager.DeselectAll(); // Clear selection if not adding
                }
            }
            else
            {
                unitManager.DeselectAll(); // Deselect if clicking empty space
            }
        }
    }

    // Handles commanding selected units to move via right mouse click
    void HandleRightClickCommand()
    {
        if (Input.GetMouseButtonDown(1) && unitManager.SelectedUnits.Count > 0)
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            // Only hit ground using the specified ground layer
            if (Physics.Raycast(ray, out RaycastHit hit, 1000, groundLayer))
            {
                // Convert world position to grid coordinates
                Vector2Int gridCoord = new Vector2Int(
                    Mathf.RoundToInt(hit.point.x),
                    Mathf.RoundToInt(hit.point.z)
                );

                GridNode[,] grid = gridManager.GetGrid();

                // Bounds check
                if (gridCoord.x < 0 || gridCoord.x >= grid.GetLength(0) || gridCoord.y < 0 || gridCoord.y >= grid.GetLength(1))
                    return;

                GridNode targetNode = grid[gridCoord.x, gridCoord.y];

                if (!targetNode.walkable) return; // Only move to walkable tiles

                // Issue move command to all currently selected units
                foreach (var unit in unitManager.SelectedUnits)
                {
                    unit.MoveTo(targetNode);
                }
            }
        }
    }
}
