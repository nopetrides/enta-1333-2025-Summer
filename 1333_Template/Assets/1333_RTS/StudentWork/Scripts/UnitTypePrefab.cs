using UnityEngine;

namespace RTS_1333
{
    /// <summary>
    /// Serializable class that pairs a UnitType ScriptableObject with a prefab for spawning.
    /// </summary>
    [System.Serializable]
    public class UnitTypePrefab
    {
        // Reference to the unit's data (size, stats, etc.)
        public UnitType unitType;
        // Reference to the prefab for this unit type
        public GameObject prefab;
    }
} 