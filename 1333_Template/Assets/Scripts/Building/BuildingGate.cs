using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class BuildingGate : BuildingInstance
{
    [SerializeField] private SkinnedMeshRenderer[] _skinnedRenderers;

    protected override void Awake()
    {
        base.Awake();
        if (_skinnedRenderers == null || _skinnedRenderers.Length == 0)
            _skinnedRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
    }

    public override void ApplyTeamMaterial()
    {
        base.ApplyTeamMaterial();
        int idx = (int)team;
        if (teamMaterials == null || idx < 0 || idx >= teamMaterials.Length) return;
        Material mat = teamMaterials[idx];
        foreach (var smr in _skinnedRenderers)
            smr.material = mat;
    }

    public override void OnSelected()
    {
        // Tint default mesh renderers (inherited behavior)
        base.OnSelected();
        // Tint skinned mesh renderers to blue
        foreach (var smr in _skinnedRenderers)
        {
            if (smr != null)
                smr.material.color = Color.blue;
        }
    }

    public override void OnDeselected()
    {
        // Revert default mesh renderers
        ApplyTeamMaterial();
        // Revert skinned mesh renderers to team material
        int idx = (int)team;
        if (teamMaterials != null && idx >= 0 && idx < teamMaterials.Length)
        {
            Material mat = teamMaterials[idx];
            foreach (var smr in _skinnedRenderers)
            {
                if (smr != null)
                    smr.material = mat;
            }
        }
    }
}