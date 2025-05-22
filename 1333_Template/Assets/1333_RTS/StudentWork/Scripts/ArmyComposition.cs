using System.Collections.Generic;
using UnityEngine;

namespace RTS_1333
{
    /// <summary>
    /// ScriptableObject that defines an army's composition: a list of unit types (with prefabs) and a count for each.
    /// </summary>
    /// 
    [CreateAssetMenu(fileName = "ArmyComposition", menuName = "Game/Army Composition")]
    public class ArmyComposition : ScriptableObject
    {
        /// <summary>
        /// Represents a single entry in the army: a unit type, its prefab, and how many to spawn.
        /// </summary>
        [System.Serializable]
        public class UnitEntry
        {
            // The type+prefab pairing for this entry.
            public UnitTypePrefab unitTypePrefab;
            // How many of this type in the army.
            public int count = 1;
        }

        // List of all unit entries in this army.
        public List<UnitEntry> units = new ();
    }
} 