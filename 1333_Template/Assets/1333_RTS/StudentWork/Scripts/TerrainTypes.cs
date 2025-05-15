using UnityEngine;

public static class TerrainTypes
{
    public static TerrainType Grass { get; private set; }
    public static TerrainType Water { get; private set; }
    public static TerrainType Mountain { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        // Create default terrain types
        Grass = ScriptableObject.CreateInstance<TerrainType>();
        Grass.name = "Grass";
        // Set properties through reflection since they're private
        var terrainType = typeof(TerrainType);
        terrainType.GetField("terrainName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(Grass, "Grass");
        terrainType.GetField("gizmoColor", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(Grass, Color.green);
        terrainType.GetField("walkable", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(Grass, true);
        terrainType.GetField("movementCost", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(Grass, 1);

        Water = ScriptableObject.CreateInstance<TerrainType>();
        Water.name = "Water";
        terrainType.GetField("terrainName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(Water, "Water");
        terrainType.GetField("gizmoColor", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(Water, Color.blue);
        terrainType.GetField("walkable", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(Water, false);
        terrainType.GetField("movementCost", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(Water, 999);

        Mountain = ScriptableObject.CreateInstance<TerrainType>();
        Mountain.name = "Mountain";
        terrainType.GetField("terrainName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(Mountain, "Mountain");
        terrainType.GetField("gizmoColor", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(Mountain, Color.gray);
        terrainType.GetField("walkable", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(Mountain, false);
        terrainType.GetField("movementCost", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(Mountain, 999);
    }
} 