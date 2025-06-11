using UnityEngine;

/// <summary>
/// Specialized building type for gates. Applies team materials to both regular
/// and skinned mesh renderers in children.
/// </summary>
[RequireComponent(typeof(Renderer))]
public class BuildingGate : BuildingInstance
{
    [Tooltip("Assign additional SkinnedMeshRenderers for gate visuals")]
    [SerializeField] private SkinnedMeshRenderer[] _skinnedRenderers;

    protected override void Awake()
    {
        base.Awake();

        // If no renderers assigned in inspector, auto-fetch from children
        if (_skinnedRenderers == null || _skinnedRenderers.Length == 0)
            _skinnedRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
    }

    /// <summary>
    /// Override base material application to handle both mesh
    /// and skinned mesh renderers.
    /// </summary>
    public override void ApplyTeamMaterial()
    {
        base.ApplyTeamMaterial();
        int index = (int)team;
        if (teamMaterials == null || index < 0 || index >= teamMaterials.Length)
            return;

        Material mat = teamMaterials[index];
        foreach (var smr in _skinnedRenderers)
        {
            if (smr != null)
                smr.material = mat;
        }
    }

    public override void OnSelected()
    {
        // TODO: show gate-specific selection visuals
    }

    public override void OnDeselected()
    {
        // TODO: hide gate-specific selection visuals
    }
}
