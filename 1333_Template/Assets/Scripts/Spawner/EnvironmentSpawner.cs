using UnityEngine;

/// <summary>
/// Spawns environment prefabs (trees, rocks) on designated terrain types at random density,
/// enforces a specified cell buffer around each instance, and supports multi-cell footprints
/// via Tree and Rock components defining Width/Height.
/// </summary>
public class EnvironmentSpawner : MonoBehaviour
{
    [Header("Grid Manager")]
    [SerializeField] private GridManager _gridManager = null;

    [Header("Terrain Types")]
    [SerializeField] private TerrainType _forestTerrainType = null;
    [SerializeField] private TerrainType _rockTerrainType = null;

    [Header("Tree Prefabs")]
    [Tooltip("List of tree prefabs; each prefab should have a Tree component specifying its width/height.")]
    [SerializeField] private GameObject[] _treePrefabs = null;

    [Header("Rock Prefabs")]
    [Tooltip("List of rock prefabs; each prefab should have a Rock component specifying its width/height.")]
    [SerializeField] private GameObject[] _rockPrefabs = null;

    [Header("Spawn Settings")]
    [Tooltip("Enable or disable tree spawning.")]
    [SerializeField] private bool _enableTreeSpawning = true;
    [Tooltip("Chance to spawn a tree on a forest tile.")]
    [Range(0f, 1f)]
    [SerializeField] private float _treeSpawnProbability = 0.5f;

    [Tooltip("Enable or disable rock spawning.")]
    [SerializeField] private bool _enableRockSpawning = true;
    [Tooltip("Chance to spawn a rock on a rock tile.")]
    [Range(0f, 1f)]
    [SerializeField] private float _rockSpawnProbability = 0.5f;

    [Header("Buffer Settings")]
    [Tooltip("Minimum empty cells required around each spawned instance.")]
    [SerializeField] private int _bufferDistance = 2;

    [Header("Spawn Parent")]
    [SerializeField] private Transform _environmentRoot = null;

    private float _cellSize;

    private void Awake()
    {
        if (_gridManager == null)
            Debug.LogError("EnvironmentSpawner: GridManager not assigned.");
        if (_forestTerrainType == null)
            Debug.LogError("EnvironmentSpawner: Forest TerrainType not assigned.");
        if (_rockTerrainType == null)
            Debug.LogError("EnvironmentSpawner: Rock TerrainType not assigned.");

        _cellSize = _gridManager.GridSettings.NodeSize;
    }

    private void Start()
    {
        SpawnEnvironment();
    }

    private void SpawnEnvironment()
    {
        int sizeX = _gridManager.GridSettings.GridSizeX;
        int sizeY = _gridManager.GridSettings.GridSizeY;
        bool[,] occupied = new bool[sizeX, sizeY];

        for (int x = 0; x < sizeX; x++)
            for (int y = 0; y < sizeY; y++)
            {
                var node = _gridManager.GetNode(x, y);
                if (node == null) continue;

                bool spawnTree = _enableTreeSpawning
                                 && node.terrainType == _forestTerrainType
                                 && _treePrefabs.Length > 0
                                 && Random.value <= _treeSpawnProbability;

                bool spawnRock = _enableRockSpawning
                                 && node.terrainType == _rockTerrainType
                                 && _rockPrefabs.Length > 0
                                 && Random.value <= _rockSpawnProbability;

                if (!spawnTree && !spawnRock) continue;

                var source = spawnTree ? _treePrefabs : _rockPrefabs;
                var prefab = source[Random.Range(0, source.Length)];

                // Determine footprint from Tree or Rock component
                int w = 1, h = 1;
                if (spawnTree)
                {
                    var tree = prefab.GetComponent<EnvTree>();
                    if (tree != null)
                    {
                        w = Mathf.Max(1, tree.Width);
                        h = Mathf.Max(1, tree.Height);
                    }
                }
                else
                {
                    var rock = prefab.GetComponent<EnvRock>();
                    if (rock != null)
                    {
                        w = Mathf.Max(1, rock.Width);
                        h = Mathf.Max(1, rock.Height);
                    }
                }

                // Skip if footprint would go out-of-bounds
                if (x + w > sizeX || y + h > sizeY) continue;

                // Skip if buffer region overlaps existing occupied cells
                if (HasBufferedNeighbor(occupied, x, y, w, h, _bufferDistance)) continue;

                SpawnAt(x, y, w, h, prefab, occupied);
            }
    }

    private bool HasBufferedNeighbor(bool[,] occupied, int startX, int startY, int w, int h, int buffer)
    {
        int minX = Mathf.Max(0, startX - buffer);
        int maxX = Mathf.Min(occupied.GetLength(0) - 1, startX + w - 1 + buffer);
        int minY = Mathf.Max(0, startY - buffer);
        int maxY = Mathf.Min(occupied.GetLength(1) - 1, startY + h - 1 + buffer);

        for (int xx = minX; xx <= maxX; xx++)
            for (int yy = minY; yy <= maxY; yy++)
                if (occupied[xx, yy])
                    return true;
        return false;
    }

    /// <summary>
    /// Instantiates the prefab centered over its footprint and marks the cells as non-walkable.
    /// </summary>
    private void SpawnAt(int startX, int startY, int w, int h, GameObject prefab, bool[,] occupied)
    {
        // Calculate ground y from bottom-left cell
        Vector3 bottomLeftWorld = new Vector3(startX * _cellSize, 0f, startY * _cellSize);
        float y = _gridManager.GetNodeFromWorldPosition(bottomLeftWorld).worldPosition.y;

        // Compute world-space width and depth
        float worldW = w * _cellSize;
        float worldH = h * _cellSize;

        // Align center using building placement logic
        float xPos = startX * _cellSize + (worldW - _cellSize) * 0.5f;
        float zPos = startY * _cellSize + (worldH - _cellSize) * 0.5f;
        Vector3 pos = new Vector3(xPos, y, zPos);

        var instance = Instantiate(prefab, pos, Quaternion.identity, _environmentRoot);

        // Mark footprint cells as non-walkable and occupied
        for (int dx = 0; dx < w; dx++)
            for (int dy = 0; dy < h; dy++)
            {
                int gx = startX + dx;
                int gy = startY + dy;
                _gridManager.SetWalkable(gx, gy, false);
                occupied[gx, gy] = true;
            }
    }
}
