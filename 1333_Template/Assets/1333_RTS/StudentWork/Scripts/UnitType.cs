using UnityEngine;


[CreateAssetMenu(fileName = "UnitType", menuName = "Game/UnitType")]   // scriptable object that defines stats and appearance for a type of unit
public class UnitType : ScriptableObject
{

    public string UnitName; // unit properties to be seen in inspector
    public Sprite UnitIcon;


    [SerializeField] private int width = 1;
    [SerializeField] private int height = 1;

    [SerializeField] private int maxHealth = 1;
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private int armor = 1;
    [SerializeField] private AttackType attackType = AttackType.Melee;
    [SerializeField] private int range = 1;

    [SerializeField] private GameObject prefab;

   
    public int Width => width;   // grid occupation
    public int Height => height;

    
    public int MaxHealth => maxHealth;  // unit stats
    public float MoveSpeed => moveSpeed;
    public int AttackDamage => attackDamage;
    public int Armor => armor;
    public int Range => range;
    public AttackType TypeOfAttack => attackType;

  
    public GameObject Prefab => prefab;    // prefab to spawn for this unit.
}
