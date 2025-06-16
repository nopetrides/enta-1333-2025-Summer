using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Banner acts as a mobile formation point. Drag to enter placement mode:
/// while holding the left mouse button, it follows the mouse cursor on the ground plane
/// and snaps to the nearest grid node. Releasing the button places it permanently at that grid node,
/// preserving its original rotation and marking that node as non-walkable.
/// Also occupies its initial grid cell at game start.
/// </summary>
[RequireComponent(typeof(Collider))]
public class Banner : MonoBehaviour
{
    [Header("Dependencies")]
    [Tooltip("Reference to the GridManager for snapping to grid.")]
    [SerializeField] private GridManager _gridManager;

    private Camera _mainCamera;
    private bool _isDragging = false;
    private Quaternion _originalRotation;
    private Plane _groundPlane;
    private GridNode _occupiedNode; // currently occupied grid node

    /// <summary>
    /// Indicates whether any Banner instance is currently being dragged.
    /// Other systems can check this to disable conflicting interactions.
    /// </summary>
    public static bool IsAnyDragging { get; private set; } = false;

    /// <summary>
    /// Cache main camera, initial rotation, set up ground plane, and validate dependencies.
    /// </summary>
    private void Awake()
    {
        _mainCamera = Camera.main;
        _originalRotation = transform.rotation;
        _groundPlane = new Plane(Vector3.up, Vector3.zero);

        if (_gridManager == null)
            Debug.LogError("Banner: GridManager is not assigned.");
    }

    /// <summary>
    /// After all Awake calls, occupy the initial grid cell.
    /// </summary>
    private void Start()
    {
        var startNode = _gridManager.getNodeFromWorldPosition(transform.position);
        OccupyNode(startNode);
    }

    /// <summary>
    /// Begin dragging when the banner is pressed.
    /// </summary>
    private void OnMouseDown()
    {
        _isDragging = true;
        IsAnyDragging = true;
    }

    /// <summary>
    /// On mouse release, place the banner at the nearest valid grid node and stop dragging.
    /// </summary>
    private void OnMouseUp()
    {
        if (!_isDragging)
            return;

        GridNode node = PlaceAtCursor();
        if (node != null)
            OccupyNode(node);

        _isDragging = false;
        IsAnyDragging = false;
    }

    /// <summary>
    /// While dragging, continuously update the banner position to follow the mouse.
    /// </summary>
    private void Update()
    {
        if (!_isDragging)
            return;

        Ray ray = _mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (_groundPlane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            GridNode node = _gridManager.getNodeFromWorldPosition(hitPoint);
            if (node != null && node.walkable)
            {
                transform.position = node.worldPosition;
                transform.rotation = _originalRotation;
            }
        }
    }

    /// <summary>
    /// Snap and place the banner at the grid node under the cursor.
    /// Returns the node if placement succeeded.
    /// </summary>
    private GridNode PlaceAtCursor()
    {
        Ray ray = _mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (_groundPlane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            GridNode node = _gridManager.getNodeFromWorldPosition(hitPoint);
            if (node != null && node.walkable)
            {
                transform.position = node.worldPosition;
                transform.rotation = _originalRotation;
                return node;
            }
        }
        return null;
    }

    /// <summary>
    /// Unmark the previous node and mark the new node as non-walkable.
    /// </summary>
    /// <param name="node">Grid node to occupy.</param>
    private void OccupyNode(GridNode node)
    {
        if (_occupiedNode != null)
            SetNodeWalkable(_occupiedNode, true);

        _occupiedNode = node;
        SetNodeWalkable(node, false);
    }

    /// <summary>
    /// Helper to set a node's walkable flag via GridManager.
    /// </summary>
    private void SetNodeWalkable(GridNode node, bool walkable)
    {
        int x = Mathf.RoundToInt(node.worldPosition.x / _gridManager.GridSettings.NodeSize);
        int y = Mathf.RoundToInt(
            _gridManager.GridSettings.UseXZPlane
                ? node.worldPosition.z / _gridManager.GridSettings.NodeSize
                : node.worldPosition.y / _gridManager.GridSettings.NodeSize
        );
        _gridManager.SetWalkable(x, y, walkable);
    }
}
