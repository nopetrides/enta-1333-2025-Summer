using UnityEngine;

// Represents each node on our grid. Brutally efficient with struct usage and flags.
[System.Serializable]
public struct GridNode
{
    public string Name; // Grid Index
    public Vector3 WorldPosition;
    public bool Walkable;
    public int Weight;

    // Future-proof: Add faction-based walkability or additional node metadata here
}