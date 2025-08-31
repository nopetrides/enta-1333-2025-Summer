using UnityEngine;


public abstract class BuildingBase : MonoBehaviour  // abstract base for all buildings in the game grid
{
   
    [SerializeField] protected BuildingType buildingType;   // building data

  
    public virtual int Width => buildingType != null ? buildingType.Width : 1;

  
    public virtual int Height => buildingType != null ? buildingType.Height : 1;


    public abstract void PlaceOnNode(GridNode node);
}