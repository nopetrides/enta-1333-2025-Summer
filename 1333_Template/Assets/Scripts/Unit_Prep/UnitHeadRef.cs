using UnityEngine;

/// <summary>
/// Assign the Renderer for the head mesh here in the Inspector.
/// </summary>
public class UnitHeadRef : MonoBehaviour
{
    [Tooltip("Drag the head's Renderer (child GameObject) here.")]
    public Renderer headRenderer = null;
}
