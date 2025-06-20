using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ResourceType", menuName = "Game/Resource Type")]
public class ResourceTypeSO : ScriptableObject
{
    [SerializeField] private List<ResourceDataSO> _resources = new();

    /// <summary>
    /// List of Resource data objects this type holds.
    /// </summary>
    public List<ResourceDataSO> Resources => _resources;
}