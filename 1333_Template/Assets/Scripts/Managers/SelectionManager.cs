using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles unit selection via click/drag and movement commands via right-click.
/// </summary>
public class SelectionManager : MonoBehaviour
{
    private Camera _mainCamera;
    private GridManager _gridManager;
    private UnitManager _unitManager;
    private UnitSelectionBox _boxDrawer;

    [SerializeField] private float _minDragSize = 3f;

    private readonly List<UnitBase> _selectedUnits = new();

    /// <summary>
    /// Initializes references for camera, grid manager, unit manager, and selection box.
    /// </summary>
    /// <param name="cam">The main camera used for raycasting.</param>
    /// <param name="gm">The grid manager used to find grid nodes.</param>
    /// <param name="um">The unit manager containing all units in the scene.</param>
    public void Initialize(Camera cam, GridManager gm, UnitManager um)
    {
        _mainCamera = cam;
        _gridManager = gm;
        _unitManager = um;
        _boxDrawer = GetComponent<UnitSelectionBox>();
        _boxDrawer.minDragSize = _minDragSize;
    }

    /// <summary>
    /// Called once per frame; toggles path gizmos and handles mouse input.
    /// </summary>
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
            UnitBase.ShowPathGizmos = !UnitBase.ShowPathGizmos;

        HandleMouseInput();
    }

    /// <summary>
    /// Processes mouse button events for starting drag, updating drag, ending drag, and issuing move commands.
    /// </summary>
    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
            _boxDrawer.BeginDrag(Mouse.current.position.ReadValue());

        if (_boxDrawer.IsDragging)
            _boxDrawer.UpdateDrag(Mouse.current.position.ReadValue());

        if (Input.GetMouseButtonUp(0) && _boxDrawer.IsDragging)
        {
            _boxDrawer.EndDrag(Mouse.current.position.ReadValue());
            if (_boxDrawer.DragDistance < _minDragSize)
                SingleClickSelect(_boxDrawer.DragEnd);
            else
                DragSelect(_boxDrawer.DragStart, _boxDrawer.DragEnd);
        }

        if (Input.GetMouseButtonDown(1) && _selectedUnits.Count > 0)
            CommandSelectedUnits();
    }

    /// <summary>
    /// Performs a single-click selection by raycasting from the clicked screen position to select a unit.
    /// </summary>
    /// <param name="screenPos">The screen coordinates where the click occurred.</param>
    private void SingleClickSelect(Vector2 screenPos)
    {
        if (_mainCamera == null) return;

        Ray ray = _mainCamera.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            ISelectable selectable = hit.collider.GetComponentInParent<ISelectable>();
            if (selectable is UnitBase unit && unit.UnitTeam == Team.Player)
            {
                ClearSelection();
                AddToSelection(unit);
                return;
            }
        }

        ClearSelection();
    }

    /// <summary>
    /// Performs a drag selection by creating a screen-space rectangle and selecting all units within it.
    /// </summary>
    /// <param name="start">Screen position where the drag started.</param>
    /// <param name="end">Screen position where the drag ended.</param>
    private void DragSelect(Vector2 start, Vector2 end)
    {
        if (_mainCamera == null || _unitManager == null) return;

        Rect rect = _boxDrawer.GetScreenRect(start, end);
        ClearSelection();

        foreach (UnitBase unit in _unitManager.AllUnits)
        {
            if (unit.UnitTeam != Team.Player) continue;

            Vector3 screenPoint = _mainCamera.WorldToScreenPoint(unit.transform.position);
            if (screenPoint.z < 0) continue;

            Vector2 guiPoint = new(screenPoint.x, Screen.height - screenPoint.y);
            if (rect.Contains(guiPoint))
                AddToSelection(unit);
        }
    }

    /// <summary>
    /// Issues move commands to all currently selected units by raycasting to the ground plane and obtaining the target grid node.
    /// </summary>
    private void CommandSelectedUnits()
    {
        if (_mainCamera == null || _gridManager == null) return;

        Ray ray = _mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        Plane ground = new(Vector3.up, Vector3.zero);

        if (ground.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            GridNode node = _gridManager.getNodeFromWorldPosition(hitPoint);
            if (!node.walkable)
            {
                Debug.Log("SelectionManager: Target node is not walkable.");
                return;
            }

            foreach (UnitBase unit in _selectedUnits)
                unit.MoveTo(node);
        }
    }

    /// <summary>
    /// Adds a unit to the selection list and displays its selection indicator.
    /// </summary>
    /// <param name="unit">The UnitBase instance to add to selection.</param>
    private void AddToSelection(UnitBase unit)
    {
        if (_selectedUnits.Contains(unit)) return;

        _selectedUnits.Add(unit);
        if (unit.TryGetComponent(out UnitVisualController unitVisualController))
        {
            unitVisualController.ShowSelectionIndicator();
        }
    }

    /// <summary>
    /// Clears the current selection by hiding all selection indicators and emptying the selected units list.
    /// </summary>
    private void ClearSelection()
    {
        foreach (var unit in _selectedUnits)
        {
            if (unit.TryGetComponent(out UnitVisualController unitVisualController))
            {
                unitVisualController.HideSelectionIndicator();
            }
        }

        _selectedUnits.Clear();
    }
}
