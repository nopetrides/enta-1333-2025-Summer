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

    // Track any ISelectable
    private readonly List<ISelectable> _selected = new List<ISelectable>();

    public void Initialize(Camera cam, GridManager gm, UnitManager um)
    {
        _mainCamera = cam;
        _gridManager = gm;
        _unitManager = um;
        _unitSelectionBox = GetComponent<UnitSelectionBox>();
        _unitSelectionBox.minDragSize = _minDragSize;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
            UnitBase.ShowPathGizmos = !UnitBase.ShowPathGizmos;
        HandleMouse();
    }

    private void HandleMouse()
    {
        if (Input.GetMouseButtonDown(0))
            _unitSelectionBox.BeginDrag(Mouse.current.position.ReadValue());
        if (_unitSelectionBox.IsDragging)
            _unitSelectionBox.UpdateDrag(Mouse.current.position.ReadValue());
        if (Input.GetMouseButtonUp(0) && _unitSelectionBox.IsDragging)
        {
            _unitSelectionBox.EndDrag(Mouse.current.position.ReadValue());
            if (_unitSelectionBox.DragDistance < _minDragSize)
                TrySingleSelect(_unitSelectionBox.DragEnd);
            else
            {
                // handle drag select for units only
                Rect selRect = _unitSelectionBox.GetScreenRect(_unitSelectionBox.DragStart, _unitSelectionBox.DragEnd);
                foreach (var unit in _unitManager.AllUnits)
                {
                    Vector3 sp = _mainCamera.WorldToScreenPoint(unit.transform.position);
                    Vector2 guiPoint = new(sp.x, Screen.height - sp.y);
                    if (selRect.Contains(guiPoint))
                        AddToSelection(unit);
                }
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            // unit move commands
            if (_selected.Count > 0)
                CommandUnits();
        }
    }

    private void TrySingleSelect(Vector2 screenPos)
    {
        ClearSelection();
        Ray ray = _mainCamera.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out var hit, 100f))
        {
            var sel = hit.collider.GetComponentInParent<ISelectable>();
            if (sel != null)
                AddToSelection(sel);
        }
    }

    private void AddToSelection(ISelectable sel)
    {
        if (_selected.Contains(sel)) return;
        _selected.Add(sel);

        if (sel is UnitBase unit)
        {
            if (unit.TryGetComponent(out UnitVisualController vc))
                vc.ShowSelectionIndicator();
        }
        else if (sel is BuildingBase building)
        {
            building.OnSelected();
        }
    }

    private void ClearSelection()
    {
        foreach (var sel in _selected)
        {
            if (sel is UnitBase unit)
            {
                if (unit.TryGetComponent(out UnitVisualController vc))
                    vc.HideSelectionIndicator();
            }
            else if (sel is BuildingBase bld)
            {
                bld.OnDeselected();
            }
        }
        _selected.Clear();
    }

    private void CommandUnits()
    {
        Ray ray = _mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        Plane ground = new Plane(Vector3.up, Vector3.zero);
        if (!ground.Raycast(ray, out var enter)) return;
        var hitPoint = ray.GetPoint(enter);
        var node = _gridManager.getNodeFromWorldPosition(hitPoint);
        if (!node.walkable) return;
        foreach (var sel in _selected)
            if (sel is UnitBase u)
                u.MoveTo(node);
    }
}