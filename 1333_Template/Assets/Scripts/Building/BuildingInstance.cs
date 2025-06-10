using UnityEngine;

/// <summary>
/// Represents a runtime instance of a building in the scene.
/// Inherits grid-snapped placement, occupancy, and selection functionality from BuildingBase.
/// </summary>
public class BuildingInstance : BuildingBase
{
    /// <summary>
    /// Instance-specific initialization after base setup.
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        // TODO: Add any additional initialization logic for instances here.
    }

    /// <summary>
    /// Called when this building is selected via the selection system.
    /// Override to show selection indicators (e.g., outline, UI panel).
    /// </summary>
    public override void OnSelected()
    {
        // TODO: Enable selection highlight or UI
    }

    /// <summary>
    /// Called when this building is deselected.
    /// Override to hide selection indicators.
    /// </summary>
    public override void OnDeselected()
    {
        // TODO: Disable selection highlight or UI
    }
}
