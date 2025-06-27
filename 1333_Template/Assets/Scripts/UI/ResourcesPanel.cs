using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Drives the UI panel that shows every resource counter.
/// Subscribes to ResourceManager events and updates the
/// corresponding TMP fields when a value changes.
/// </summary>
public class ResourcePanelUI : MonoBehaviour
{
    [SerializeField] private ResourceManager _manager = null;

    [Header("TMP fields")]
    [Tooltip("Bread amount text")]
    [SerializeField] private TMP_Text _breadText = null;

    [Tooltip("Rock amount text")]
    [SerializeField] private TMP_Text _rockText = null;

    [Tooltip("Iron amount text")]
    [SerializeField] private TMP_Text _ironText = null;

    [Tooltip("Wood amount text")]
    [SerializeField] private TMP_Text _woodText = null;

    [Tooltip("Unit capacity or unit resource text")]
    [SerializeField] private TMP_Text _unitText = null;

    /// <summary>
    /// Runtime lookup: Resource enum → TMP text component.
    /// </summary>
    private Dictionary<ResourceList, TMP_Text> _map;

    private void Awake()
    {
        // Build the enum-to-text map once.
        _map = new Dictionary<ResourceList, TMP_Text>
        {
            { ResourceList.Bread, _breadText },
            { ResourceList.Rock,  _rockText  },
            { ResourceList.Iron,  _ironText  },
            { ResourceList.Wood,  _woodText  },
            { ResourceList.Units, _unitText  }
        };
    }

    private void OnEnable()
    {
        if (_manager == null) return;

        _manager.OnResourceChanged += HandleChanged;
    }

    private void OnDisable()
    {
        if (_manager == null) return;

        _manager.OnResourceChanged -= HandleChanged;
    }

    /// <summary>
    /// Called by the ResourceManager whenever a single resource value changes.
    /// Updates the matching TMP text if it exists in the map.
    /// </summary>
    private void HandleChanged(ResourceList type, int newValue)
    {
        if (_map.TryGetValue(type, out var txt) && txt != null)
            txt.text = newValue.ToString();
    }

    /// <summary>
    /// Fills all counters with the current totals
    /// (used once when the panel becomes active).
    /// </summary>
    public void RefreshAll()
    {
        foreach (var kv in _map)
        {
            if (kv.Value == null) continue;

            int count = _manager.TryGetResourceCount(kv.Key);
            kv.Value.text = count.ToString();
        }
    }
}
