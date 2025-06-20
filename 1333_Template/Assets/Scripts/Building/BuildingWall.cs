using UnityEngine;

/// <summary>
/// Represents a placed building; handles selection coloring on specified renderers.
/// </summary>
public class BuildingWall : BuildingBase
{
    [Header("Renderers for Selection")]
    [Tooltip("Assign specific mesh renderers to tint on selection")]
    [SerializeField] private Renderer[] _selectionRenderers;

    protected override void Awake()
    {
        base.Awake();
        // Auto-populate if not assigned
        if (_selectionRenderers == null || _selectionRenderers.Length == 0)
            _selectionRenderers = GetComponentsInChildren<Renderer>();
    }

    public override void OnSelected()
    {
        // Change only specified mesh renderers to blue
        foreach (var r in _selectionRenderers)
        {
            if (r != null)
                r.material.color = Color.gray;
        }
    }

    public override void OnDeselected()
    {
        // Revert to original team material
        ApplyTeamMaterial();
    }
}
