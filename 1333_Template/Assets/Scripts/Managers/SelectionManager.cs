using System.Collections.Generic;
using UnityEngine.InputSystem;

using UnityEngine;

/// <summary>
/// Handles selection of units and buildings (ISelectable).
/// </summary>
public class SelectionManager : MonoBehaviour
{
    private Camera _mainCamera;
    private GridManager _gridManager;
    private UnitManager _unitManager;
    private UnitSelectionBox _unitSelectionBox;
    [SerializeField] private float _minDragSize = 3f;

    [Header("UI Manager")]
    [Tooltip("Central SelectedUIManager for all panels.")]
    [SerializeField] private SelectedUIManager _uiManager = null;


    // Track any ISelectable
    private readonly List<ISelectable> _selected = new List<ISelectable>();

    /// <summary>
    /// Initializes the SelectionManager with required dependencies.
    /// </summary>
    public void Initialize(Camera cam, GridManager gm, UnitManager um)
    {
        _mainCamera = cam;
        _gridManager = gm;
        _unitManager = um;
        _unitSelectionBox = GetComponent<UnitSelectionBox>();
        _unitSelectionBox.minDragSize = _minDragSize;

        //Ensure All selectedUI panels are hided
        _uiManager.HideAll();
    }

    /// <summary>
    /// Called once per frame. Handles toggling gizmos and mouse input for selection.
    /// </summary>
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
            UnitMovement.ShowPathGizmos = !UnitMovement.ShowPathGizmos;  

        HandleMouse();
    }

    /// <summary>
    /// Handles mouse input for selection and commanding units.
    /// </summary>
    private void HandleMouse()
    {
        if (Banner.IsAnyDragging)
            return;
        if (Input.GetMouseButtonDown(0))
            _unitSelectionBox.BeginDrag(Mouse.current.position.ReadValue());

        if (_unitSelectionBox.IsDragging)
            _unitSelectionBox.UpdateDrag(Mouse.current.position.ReadValue());

        if (Input.GetMouseButtonUp(0) && _unitSelectionBox.IsDragging)
        {
            _unitSelectionBox.EndDrag(Mouse.current.position.ReadValue());
            ClearSelection();
            if (_unitSelectionBox.DragDistance < _minDragSize)
            {
                TrySingleSelect(_unitSelectionBox.DragEnd);
            }

            else
            {

                // handle drag select for units only
                Rect selRect = _unitSelectionBox.GetScreenRect(_unitSelectionBox.DragStart, _unitSelectionBox.DragEnd);
                foreach (var unit in _unitManager.AllUnits)
                {
                    // skip any non-player team units
                    if (unit.UnitTeam != Team.Player)
                        continue;
                    Vector3 sp = _mainCamera.WorldToScreenPoint(unit.transform.position);
                    Vector2 guiPoint = new(sp.x, Screen.height - sp.y);
                    if (selRect.Contains(guiPoint))
                    {
                        AddToSelection(unit);
                    }
                }

                // only show panel if exactly one unit was dragged over
                if (_selected.Count == 1)
                    _uiManager.Show(_selected[0]);
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            // unit move commands
            if (_selected.Count > 0)
            {
                CommandUnits();
            }
        }
    }

    /// <summary>
    /// Tries to select a single selectable object at the given screen position.
    /// </summary>
    private void TrySingleSelect(Vector2 screenPos)
    {
        ClearSelection();
        Ray ray = _mainCamera.ScreenPointToRay(screenPos);

        if (Physics.Raycast(ray, out var hit, 100f))
        {
            var sel = hit.collider.GetComponentInParent<ISelectable>();
            if (sel != null)
                AddToSelection(sel);
            // single‐click: exactly one item => show its panel
            _uiManager.Show(sel);
        }
    }

    /// <summary>
    /// Adds the given ISelectable to the selection and shows its visual feedback.
    /// </summary>
    private void AddToSelection(ISelectable sel)
    {
        if (sel is UnitBase unit && unit.UnitTeam != Team.Player)
            return;
        if (sel is BuildingBase b && b.team != Team.Player)
            return;
        if (_selected.Contains(sel)) return;
        _selected.Add(sel);

        // unified selection hook + UI panel
        sel.OnSelected();
    }

    /// <summary>
    /// Clears the current selection and hides visual indicators.
    /// </summary>
    private void ClearSelection()
    {
        _uiManager.HideAll();

        foreach (var sel in _selected)
            sel.OnDeselected();

        _selected.Clear();
    }

    /// <summary>
    /// Commands all selected units to move to the target grid node.
    /// Frees each unit's starting cell before computing any paths.
    /// </summary>
    private void CommandUnits()
    {
        Ray ray = _mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        Plane ground = new Plane(Vector3.up, Vector3.zero);
        if (!ground.Raycast(ray, out var enter)) return;

        Vector3 hitPoint = ray.GetPoint(enter);
        GridNode targetNode = _gridManager.getNodeFromWorldPosition(hitPoint);
        if (!targetNode.walkable) return;

        // Gather selected units
        var units = new List<UnitBase>();
        foreach (var sel in _selected)
            if (sel is UnitBase unit)
                units.Add(unit);

        // 1) Free all start cells so no unit blocks pathfinding
        foreach (var u in units)
        {
            GridNode startNode = _gridManager.getNodeFromWorldPosition(u.transform.position);
            startNode.walkable = true;
        }

        // 2) Find nearest free nodes around the target for each unit
        var assignedNodes = _gridManager.FindNearestFreeNodes(targetNode, units.Count);

        // 3) Issue movement commands
        for (int i = 0; i < units.Count; i++)
        {
            var u = units[i];
            var destNode = assignedNodes[i];
            u.MoveTo(destNode);
        }
    }
}
