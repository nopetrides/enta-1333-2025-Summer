using System;
using UnityEngine;

[Serializable]
public struct ResourceCost
{
    /// <summary>Type of resource to spend.</summary>
    public ResourceList ResourceType;
    /// <summary>Amount required to build.</summary>
    public int Amount;
}