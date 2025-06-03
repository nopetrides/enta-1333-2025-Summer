// SelectionManager.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles both single-click and drag-box selection of units that implement ISelectable,
/// plus group move commands on right-click. Uses a UnitManager reference (injected)
/// to iterate over all active units instead of scanning the scene. Also toggles
/// path gizmos on/off when X is pressed.
/// </summary>
public class SelectionManager : MonoBehaviour
{
    private Camera _mainCamera;
    private GridManager _gridManager;
    private UnitManager _unitManager;

    // List of currently selected units (only Team.Player).
    private readonly List<UnitBase> _selectedUnits = new List<UnitBase>();

    // For mouse-drag selection:
    private bool _isDragging = false;
    private Vector2 _dragStartPos;
    private Vector2 _dragCurrentPos;

    // 1×1 white texture used for drawing the selection rectangle in OnGUI().
    private static Texture2D _selectionTexture;

    /// <summary>
    /// Initializes this manager with required references.
    /// Must be called from GameManager.Awake().
    /// </summary>
    /// <param name="cam">The main RTS camera used for raycasts.</param>
    /// <param name="gm">Reference to the GridManager.</param>
    /// <param name="um">Reference to the UnitManager.</param>
    public void Initialize(Camera cam, GridManager gm, UnitManager um)
    {
        _mainCamera = cam;
        _gridManager = gm;
        _unitManager = um;

        if (_selectionTexture == null)
        {
            _selectionTexture = new Texture2D(1, 1);
            _selectionTexture.SetPixel(0, 0, Color.white);
            _selectionTexture.Apply();
        }
    }

    private void Update()
    {
        // Toggle path gizmo drawing when X is pressed
        if (Input.GetKeyDown(KeyCode.X))
        {
            UnitBase.ShowPathGizmos = !UnitBase.ShowPathGizmos;
        }

        // Left mouse button down: begin drag or click
        if (Input.GetMouseButtonDown(0))
        {
            _dragStartPos = Mouse.current.position.ReadValue();
            _isDragging = true;
        }

        // While holding left mouse button: update current drag position
        if (_isDragging)
        {
            _dragCurrentPos = Mouse.current.position.ReadValue();
        }

        // Left mouse button up: perform selection logic
        if (Input.GetMouseButtonUp(0) && _isDragging)
        {
            _dragCurrentPos = Mouse.current.position.ReadValue();
            _isDragging = false;

            float dragDistance = Vector2.Distance(_dragStartPos, _dragCurrentPos);
            if (dragDistance < 5f)
            {
                SingleClickSelect(_dragCurrentPos);
            }
            else
            {
                DragSelectGroup(_dragStartPos, _dragCurrentPos);
            }
        }

        // Right mouse button down: command selected units to move
        if (Input.GetMouseButtonDown(1) && _selectedUnits.Count > 0)
        {
            CommandSelectedUnits();
        }
    }

    /// <summary>
    /// Handles a single-click selection at the given screen position.
    /// Raycasts to find a unit that implements ISelectable and belongs to Team.Player.
    /// Selects it or deselects all if none hit.
    /// </summary>
    /// <param name="screenPos">Screen-space mouse position.</param>
    private void SingleClickSelect(Vector2 screenPos)
    {
        if (_mainCamera == null)
            return;

        Ray ray = _mainCamera.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, 100f))
        {
            ISelectable selectable = hitInfo.collider.GetComponentInParent<ISelectable>();
            if (selectable != null)
            {
                if (selectable is UnitBase unit && unit.UnitTeam == Team.Player)
                {
                    ClearSelection();
                    AddToSelection(unit);
                    return;
                }
            }
        }

        // If no valid unit was hit, clear selection.
        ClearSelection();
    }

    /// <summary>
    /// Handles a drag-box selection between two screen positions.
    /// Finds all Team.Player units whose screen positions lie within the rectangle.
    /// </summary>
    /// <param name="start">Drag start screen position.</param>
    /// <param name="end">Drag end screen position.</param>
    private void DragSelectGroup(Vector2 start, Vector2 end)
    {
        if (_mainCamera == null || _unitManager == null)
            return;

        Rect rect = GetScreenRect(start, end);
        ClearSelection();

        foreach (UnitBase unit in _unitManager.AllUnits)
        {
            if (unit.UnitTeam != Team.Player)
                continue;

            Vector3 worldPos = unit.transform.position;
            Vector3 screenPoint = _mainCamera.WorldToScreenPoint(worldPos);

            // Ignore units behind the camera
            if (screenPoint.z < 0)
                continue;

            // Convert to GUI space (origin at top-left)
            Vector2 guiPoint = new Vector2(screenPoint.x, Screen.height - screenPoint.y);
            if (rect.Contains(guiPoint))
            {
                AddToSelection(unit);
            }
        }
    }

    /// <summary>
    /// Issues MoveTo() on every selected unit when right-clicking on the grid.
    /// Converts the click to a GridNode and commands all selected units.
    /// </summary>
    private void CommandSelectedUnits()
    {
        if (_mainCamera == null || _gridManager == null)
            return;

        Ray ray = _mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

        if (groundPlane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            GridNode targetNode = _gridManager.getNodeFromWorldPosition(hitPoint);

            if (!targetNode.walkable)
            {
                Debug.Log("SelectionManager: Target node is not walkable.");
                return;
            }

            foreach (UnitBase unit in _selectedUnits)
            {
                unit.MoveTo(targetNode);
            }
        }
    }

    /// <summary>
    /// Adds a unit to the current selection and highlights it (e.g., change head color to yellow).
    /// </summary>
    /// <param name="unit">The unit to add.</param>
    private void AddToSelection(UnitBase unit)
    {
        if (_selectedUnits.Contains(unit))
            return;

        _selectedUnits.Add(unit);
        UnitHeadRef head = unit.GetComponent<UnitHeadRef>();
        if (head != null && head.headRenderer != null)
        {
            head.headRenderer.material.color = Color.yellow;
        }
    }

    /// <summary>
    /// Clears all currently selected units and restores their original team materials.
    /// </summary>
    private void ClearSelection()
    {
        foreach (UnitBase unit in _selectedUnits)
        {
            UnitHeadRef head = unit.GetComponent<UnitHeadRef>();
            if (head != null && head.headRenderer != null)
            {
                Material teamMat = unit.UnitType.GetArmyMaterial(unit.UnitTeam);
                head.headRenderer.material = teamMat;
            }
        }

        _selectedUnits.Clear();
    }

    /// <summary>
    /// Returns a Rect in GUI space given two screen-space corners (where y is from bottom).
    /// GUI space has origin at top-left, so we flip y accordingly.
    /// </summary>
    private Rect GetScreenRect(Vector2 screenPos1, Vector2 screenPos2)
    {
        Vector2 p1 = new Vector2(screenPos1.x, Screen.height - screenPos1.y);
        Vector2 p2 = new Vector2(screenPos2.x, Screen.height - screenPos2.y);

        float xMin = Mathf.Min(p1.x, p2.x);
        float yMin = Mathf.Min(p1.y, p2.y);
        float width = Mathf.Abs(p1.x - p2.x);
        float height = Mathf.Abs(p1.y - p2.y);

        return new Rect(xMin, yMin, width, height);
    }

    /// <summary>
    /// Draws the current drag rectangle in OnGUI when _isDragging is true.
    /// </summary>
    private void OnGUI()
    {
        if (!_isDragging)
            return;

        Rect rect = GetScreenRect(_dragStartPos, _dragCurrentPos);

        Color prevColor = GUI.color;
        GUI.color = new Color(0f, 0.5f, 1f, 0.2f);
        GUI.DrawTexture(rect, _selectionTexture);

        GUI.color = Color.white;
        GUI.Box(rect, GUIContent.none);

        GUI.color = prevColor;
    }
}
