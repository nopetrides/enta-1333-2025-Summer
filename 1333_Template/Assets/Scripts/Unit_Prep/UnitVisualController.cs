// UnitHeadRef.cs
using UnityEngine;

/// <summary>
/// Holds references to the head, body, horse renderers, and a selection indicator GameObject.
/// Provides a method to apply a team material and to show/hide the selection indicator.
/// </summary>
public class UnitVisualController : MonoBehaviour
{
    [Tooltip("Drag the head's Renderer (child GameObject) here.")]
    public Renderer headRenderer = null;

    [Tooltip("Drag the body's Renderer (child GameObject) here.")]
    public Renderer bodyRenderer = null;

    [Tooltip("Drag the horse's Renderer (child GameObject) here.")]
    public Renderer horseRenderer = null;

    [Tooltip("Drag the selection indicator GameObject (e.g., a ring or highlight) here.")]
    public GameObject selectionIndicator = null;

    private void Awake()
    {
        // Ensure the selection indicator is initially disabled
        if (selectionIndicator != null)
        {
            selectionIndicator.SetActive(false);
        }
    }

    /// <summary>
    /// Apply the given material to head and body, and to horse if mounted is true.
    /// </summary>
    /// <param name="teamMaterial">Material to assign to head, body, and optionally horse.</param>
    /// <param name="mounted">Whether to apply the material to the horse renderer.</param>
    public void ApplyTeamMaterial(Material teamMaterial, bool mounted)
    {
        if (headRenderer != null)
        {
            headRenderer.material = teamMaterial;
        }

        if (bodyRenderer != null)
        {
            bodyRenderer.material = teamMaterial;
        }

        if (mounted && horseRenderer != null)
        {
            horseRenderer.material = teamMaterial;
        }
    }

    /// <summary>
    /// Enable (show) the selection indicator.
    /// </summary>
    public void ShowSelectionIndicator()
    {
        if (selectionIndicator != null)
        {
            selectionIndicator.SetActive(true);
        }
    }

    /// <summary>
    /// Disable (hide) the selection indicator.
    /// </summary>
    public void HideSelectionIndicator()
    {
        if (selectionIndicator != null)
        {
            selectionIndicator.SetActive(false);
        }
    }
}
