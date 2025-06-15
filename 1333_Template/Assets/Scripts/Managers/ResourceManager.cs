using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages all game resources defined in ResourceTypeSO collections.
/// Tracks counts for each ResourceDataSO and provides methods to add, spend,
/// and query resources using either ResourceDataSO or ResourceList enum.
/// Displays a debug list of resources in the Inspector when in the Editor.
/// </summary>
public class ResourceManager : MonoBehaviour
{
    [Header("All Resource Types")]
    [Tooltip("List of ResourceTypeSO assets to initialize resource entries.")]
    [SerializeField] private List<ResourceTypeSO> _resourceTypeSOs = new();

    // Internal dictionary mapping each ResourceDataSO to its current count.
    private Dictionary<ResourceDataSO, int> _resources;
    // Lookup map from enum value to ResourceDataSO asset for enum-based methods.
    private Dictionary<ResourceList, ResourceDataSO> _enumLookup;

#if UNITY_EDITOR
    [Header("Debug: Resource Dictionary")]
    [Tooltip("Read-only list of resources and their counts for debugging.")]
    [SerializeField] private List<string> _debugResourceList = new List<string>();
#endif

    /// <summary>
    /// Initializes the manager by creating entries for every ResourceDataSO
    /// in each listed ResourceTypeSO and builds enum lookup.
    /// Must be called once at game start.
    /// </summary>
    public void Initialize()
    {
        _resources = new Dictionary<ResourceDataSO, int>();
        _enumLookup = new Dictionary<ResourceList, ResourceDataSO>();

        foreach (var typeSO in _resourceTypeSOs)
        {
            foreach (var data in typeSO.Resources)
            {
                if (data == null)
                    continue;

                // Initialize resource count
                if (!_resources.ContainsKey(data))
                    _resources[data] = 0;

                // Populate enum lookup
                var key = data.ResourceType;
                if (!_enumLookup.ContainsKey(key))
                    _enumLookup[key] = data;
            }
        }

#if UNITY_EDITOR
        UpdateDebugList();
#endif
    }

    /// <summary>
    /// Adds the specified amount of the given ResourceDataSO.
    /// </summary>
    /// <param name="data">The ResourceDataSO representing the type to add.</param>
    /// <param name="amount">The amount of units to add.</param>
    public void AddResource(ResourceDataSO data, int amount)
    {
        if (data == null)
            return;

        if (!_resources.ContainsKey(data))
            _resources[data] = 0;

        _resources[data] += amount;
        Debug.Log($"ResourceManager: Added {amount}x {data.DisplayName}. New total: {_resources[data]}");

#if UNITY_EDITOR
        UpdateDebugList();
#endif
    }

    /// <summary>
    /// Adds the specified amount of the resource identified by enum.
    /// </summary>
    /// <param name="type">The ResourceList enum identifying the resource.</param>
    /// <param name="amount">The amount of units to add.</param>
    public void TryAddResource(ResourceList type, int amount)
    {
        if (_enumLookup.TryGetValue(type, out var data))
            AddResource(data, amount);
    }

    /// <summary>
    /// Attempts to spend the specified amount of the given ResourceDataSO.
    /// Returns true if successful; false if insufficient.
    /// </summary>
    /// <param name="data">The ResourceDataSO representing the type to spend.</param>
    /// <param name="amount">The amount of units to spend.</param>
    /// <returns>True if the spend was successful; otherwise false.</returns>
    public bool SpendResource(ResourceDataSO data, int amount)
    {
        if (data == null || !_resources.ContainsKey(data) || _resources[data] < amount)
            return false;

        _resources[data] -= amount;
        Debug.Log($"ResourceManager: Spent {amount}x {data.DisplayName}. Remaining: {_resources[data]}");

#if UNITY_EDITOR
        UpdateDebugList();
#endif
        return true;
    }

    /// <summary>
    /// Attempts to spend the specified amount of the resource identified by enum.
    /// Returns true if successful; false if insufficient.
    /// </summary>
    /// <param name="type">The ResourceList enum identifying the resource.</param>
    /// <param name="amount">The amount of units to spend.</param>
    /// <returns>True if the spend was successful; otherwise false.</returns>
    public bool TrySpendResource(ResourceList type, int amount)
    {
        if (_enumLookup.TryGetValue(type, out var data))
            return SpendResource(data, amount);
        return false;
    }

    /// <summary>
    /// Gets the current count for the specified ResourceDataSO.
    /// </summary>
    /// <param name="data">The ResourceDataSO to query.</param>
    /// <returns>Current count of the resource, or zero if not tracked.</returns>
    public int GetResourceCount(ResourceDataSO data)
    {
        if (data == null)
            return 0;

        return _resources.TryGetValue(data, out var count) ? count : 0;
    }

    /// <summary>
    /// Gets the current count for the resource identified by enum.
    /// </summary>
    /// <param name="type">The ResourceList enum identifying the resource.</param>
    /// <returns>Current count, or zero if not tracked.</returns>
    public int TryGetResourceCount(ResourceList type)
    {
        if (_enumLookup.TryGetValue(type, out var data))
            return GetResourceCount(data);
        return 0;
    }

    /// <summary>
    /// Provides a read-only view of all resource counts keyed by ResourceDataSO.
    /// </summary>
    public IReadOnlyDictionary<ResourceDataSO, int> GetAllResources() => _resources;

#if UNITY_EDITOR
    /// <summary>
    /// Updates the debug list shown in the Inspector.
    /// </summary>
    private void UpdateDebugList()
    {
        _debugResourceList.Clear();
        foreach (var kv in _resources)
        {
            var name = kv.Key != null ? kv.Key.DisplayName : "Unknown";
            _debugResourceList.Add($"{name}: {kv.Value}");
        }
    }

    private void OnValidate()
    {
        if (_resources != null)
            UpdateDebugList();
    }
#endif
}
