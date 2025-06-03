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

    [SerializeField] private float minDragSize = 3f;

    private readonly List<UnitBase> _selectedUnits = new();

    public void Initialize(Camera cam, GridManager gm, UnitManager um)
    {
        _mainCamera = cam;
        _gridManager = gm;
        _unitManager = um;
        _boxDrawer = GetComponent<UnitSelectionBox>();
        _boxDrawer.minDragSize = minDragSize;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
            UnitBase.ShowPathGizmos = !UnitBase.ShowPathGizmos;

        HandleMouseInput();
    }

    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
            _boxDrawer.BeginDrag(Mouse.current.position.ReadValue());

        if (_boxDrawer.IsDragging)
            _boxDrawer.UpdateDrag(Mouse.current.position.ReadValue());

        if (Input.GetMouseButtonUp(0) && _boxDrawer.IsDragging)
        {
            _boxDrawer.EndDrag(Mouse.current.position.ReadValue());
            if (_boxDrawer.DragDistance < minDragSize)
                SingleClickSelect(_boxDrawer.DragEnd);
            else
                DragSelect(_boxDrawer.DragStart, _boxDrawer.DragEnd);
        }

        if (Input.GetMouseButtonDown(1) && _selectedUnits.Count > 0)
            CommandSelectedUnits();
    }

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

    private void AddToSelection(UnitBase unit)
    {
        if (_selectedUnits.Contains(unit)) return;

        _selectedUnits.Add(unit);
        if (unit.TryGetComponent(out UnitHeadRef head) && head.headRenderer != null)
            head.headRenderer.material.color = Color.yellow;
    }

    private void ClearSelection()
    {
        foreach (var unit in _selectedUnits)
        {
            if (unit.TryGetComponent(out UnitHeadRef head) && head.headRenderer != null)
                head.headRenderer.material = unit.UnitType.GetArmyMaterial(unit.UnitTeam);
        }

        _selectedUnits.Clear();
    }
}
