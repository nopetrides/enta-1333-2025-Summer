using UnityEngine;

/// <summary>
/// Spawns environment prefabs (trees, rocks) on designated terrain types at random density,
/// enforces a specified cell buffer around each instance, supports multi-cell footprints,
/// and injects UnitManager dependency into spawned EnvTree/EnvRock instances.
/// </summary>
public class EnvironmentSpawner : MonoBehaviour
{
    [Header("Dependencies")]
    [Tooltip("Reference to the UnitManager for DI into resources.")]
    [SerializeField] private UnitManager _unitManager = null;
    [SerializeField] private ResourceManager _resourceManager = null;

    [Header("Banner Spawn")]
    [SerializeField] private bool _spawnBanner = true;
    [Tooltip("Banner prefab with Banner component.")]
    [SerializeField] private GameObject _bannerPrefab = null;
    [Tooltip("Grid coordinates to place the banner (-1 = auto-center).")]
    [SerializeField] private Vector2Int _bannerGridPos = new Vector2Int(-1, -1);

    [Header("Grid Manager")]
    [SerializeField] private GridManager _gridManager = null;

    [Header("Terrain Types")]
    [SerializeField] private TerrainType _forestTerrainType = null;
    [SerializeField] private TerrainType _rockTerrainType = null;

    [Header("Tree Prefabs")]
    [Tooltip("List of EnvTree prefabs; each prefab must have EnvTree component with Width/Height and Initialize(UnitManager).")]
    [SerializeField] private GameObject[] _treePrefabs = null;

    [Header("Rock Prefabs")]
    [Tooltip("List of EnvRock prefabs; each prefab must have EnvRock component with Width/Height and Initialize(UnitManager).")]
    [SerializeField] private GameObject[] _rockPrefabs = null;

    [Header("Spawn Toggles & Probabilities")]
    [SerializeField] private bool _enableTreeSpawning = true;
    [Range(0f, 1f)]
    [SerializeField] private float _treeSpawnProbability = 0.5f;
    [SerializeField] private bool _enableRockSpawning = true;
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
        // Validate dependencies
        if (_unitManager == null)
            Debug.LogError("EnvironmentSpawner: UnitManager not assigned.");
        if (_gridManager == null)
            Debug.LogError("EnvironmentSpawner: GridManager not assigned.");
        if (_forestTerrainType == null)
            Debug.LogError("EnvironmentSpawner: Forest TerrainType not assigned.");
        if (_rockTerrainType == null)
            Debug.LogError("EnvironmentSpawner: Rock TerrainType not assigned.");

        // Cache cell size for footprint calculations
        _cellSize = _gridManager.GridSettings.NodeSize;
    }

    public void InitializeEnvironment()
    {
        SpawnEnvironment();
        if (_spawnBanner) SpawnBanner();
    }

    private void SpawnBanner()
    {
        if (_bannerPrefab == null)
        {
            Debug.LogWarning("EnvironmentSpawner: Banner prefab not assigned.");
            return;
        }

        int gx = _bannerGridPos.x >= 0
                 ? _bannerGridPos.x
                 : _gridManager.GridSettings.GridSizeX / 2;
        int gy = _bannerGridPos.y >= 0
                 ? _bannerGridPos.y
                 : _gridManager.GridSettings.GridSizeY / 2;

        GridNode node = _gridManager.GetNode(gx, gy);
        if (node == null || !node.walkable)
        {
            Debug.LogWarning($"EnvironmentSpawner: Invalid banner node ({gx},{gy}).");
            return;
        }

        var bannerGO = Instantiate(_bannerPrefab,
                                   node.worldPosition,
                                   Quaternion.Euler(0f, 180f, 0f),
                                   _environmentRoot);

        if (bannerGO.TryGetComponent<Banner>(out var banner))
        {
            // DI
            banner.Initialize(_gridManager);
        }
        else
        {
            Debug.LogError("EnvironmentSpawner: Banner prefab missing Banner component.");
        }

        // Mark that grid cell as non-walkable immediately
        _gridManager.SetWalkable(gx, gy, false);
    }

    /// <summary>
    /// Iterate grid nodes, randomly spawn trees/rocks with buffer enforcement,
    /// and inject UnitManager into each spawned EnvTree/EnvRock.
    /// </summary>
    private void SpawnEnvironment()
    {
        int sizeX = _gridManager.GridSettings.GridSizeX;
        int sizeY = _gridManager.GridSettings.GridSizeY;
        bool[,] occupied = new bool[sizeX, sizeY];

        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                var node = _gridManager.GetNode(x, y);
                if (node == null) continue;

                bool spawnTree = _enableTreeSpawning && node.terrainType == _forestTerrainType
                                 && _treePrefabs.Length > 0 && Random.value <= _treeSpawnProbability;

                bool spawnRock = _enableRockSpawning && node.terrainType == _rockTerrainType
                                 && _rockPrefabs.Length > 0 && Random.value <= _rockSpawnProbability;

                if (!spawnTree && !spawnRock) continue;

                var source = spawnTree ? _treePrefabs : _rockPrefabs;
                var prefab = source[Random.Range(0, source.Length)];

                // Get footprint dimensions from component
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

                // Skip out-of-bounds or overlapping buffer
                if (x + w > sizeX || y + h > sizeY) continue;
                if (HasBufferedNeighbor(occupied, x, y, w, h, _bufferDistance)) continue;

                SpawnAt(x, y, w, h, prefab, occupied);
            }
        }
    }

    /// <summary>
    /// Returns true if any occupied cell exists within the footprint extended by buffer distance.
    /// </summary>
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
    /// Instantiate the prefab centered over its footprint, mark grid cells, and inject UnitManager.
    /// </summary>
    private void SpawnAt(int startX, int startY, int w, int h, GameObject prefab, bool[,] occupied)
    {
        // Compute ground Y from bottom-left cell center
        Vector3 bottomLeft = new Vector3(startX * _cellSize, 0f, startY * _cellSize);
        float groundY = _gridManager.GetNodeFromWorldPosition(bottomLeft).worldPosition.y;

        // Compute world-space footprint size
        float worldW = w * _cellSize;
        float worldH = h * _cellSize;

        // Center position calculation
        float xPos = startX * _cellSize + (worldW - _cellSize) * 0.5f;
        float zPos = startY * _cellSize + (worldH - _cellSize) * 0.5f;
        Vector3 spawnPos = new Vector3(xPos, groundY, zPos);

        var instance = Instantiate(prefab, spawnPos, Quaternion.identity, _environmentRoot);

        // Mark cells as non-walkable and occupied
        for (int dx = 0; dx < w; dx++)
            for (int dy = 0; dy < h; dy++)
            {
                int gx = startX + dx;
                int gy = startY + dy;
                _gridManager.SetWalkable(gx, gy, false);
                occupied[gx, gy] = true;
            }

        // dependency injection and grid info setup
        if (instance.TryGetComponent<EnvTree>(out var treeComp))
        {
            treeComp.Initialize(_unitManager, _resourceManager);
            treeComp.SetGridInfo(_gridManager, startX, startY);
        }
        else if (instance.TryGetComponent<EnvRock>(out var rockComp))
        {
            rockComp.Initialize(_unitManager, _resourceManager);
            rockComp.SetGridInfo(_gridManager, startX, startY);
        }
    }
}
