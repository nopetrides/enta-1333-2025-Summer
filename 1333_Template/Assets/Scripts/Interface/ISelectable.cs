/// <summary>
/// Marker interface for any selectable game object.
/// Now includes selection callbacks.
/// </summary>
public interface ISelectable
{
    /// <summary>Called once when this object is selected.</summary>
    void OnSelected();

    /// <summary>Called once when this object is deselected.</summary>
    void OnDeselected();
}
