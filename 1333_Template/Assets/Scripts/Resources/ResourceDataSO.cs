using UnityEngine;

[CreateAssetMenu(fileName = "ResourceData", menuName = "Game/Resource Data")]
public class ResourceDataSO : ScriptableObject
{
    [Header("Type Info")]
    [Tooltip("Select the resource type from the list.")]
    [SerializeField] private ResourceList _resourceType;

    [Tooltip("Display name for this resource.")]
    [SerializeField] private string _displayName;

    [Tooltip("Icon for this resource.")]
    [SerializeField] private Sprite _icon;

    [Tooltip("Description for this resource.")]
    [SerializeField] private string _description;
    /// <summary>
    /// Gets the enum type of this resource.
    /// </summary>
    public ResourceList ResourceType => _resourceType;

    /// <summary>
    /// Gets the display name of this resource.
    /// </summary>
    public string DisplayName => _displayName;

    /// <summary>
    /// Gets the icon sprite of this resource.
    /// </summary>
    public Sprite Icon => _icon;

    /// <summary>
    /// Gets the description of this resource.
    /// </summary>
    public string Description => _description;
}
