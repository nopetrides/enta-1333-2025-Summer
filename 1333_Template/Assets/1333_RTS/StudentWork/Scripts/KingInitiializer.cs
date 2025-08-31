using UnityEngine;


public class KingInitializer : MonoBehaviour  // initializer in game 
{
    [SerializeField] private AStarPathfinder pathfinder;
    [SerializeField] private Material kingMaterial;
    [SerializeField] private int kingTeam = 0;

    void Start()
    {
        var unit = GetComponent<UnitInstance>();
        if (unit != null)
        {
            unit.Initialize(pathfinder, kingMaterial, kingTeam);
        }
     
    }

    
    }
