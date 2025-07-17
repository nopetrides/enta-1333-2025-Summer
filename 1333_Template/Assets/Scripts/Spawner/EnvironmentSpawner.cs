using UnityEngine;

/// <summary>
/// Spawns environment prefabs (trees, rocks) and (optionally) the main Castle building.
/// Castle is spawned at grid center (full footprint) before any environment objects,
/// using BuildingPlacementManager-style snap + grid occupancy. Castle keeps prefab rotation.
/// </summary>
public class EnvironmentSpawner : MonoBehaviour
{
    /* ------------------------------------------------------------------ */
    /*  Dependencies                                                      */
    /* ------------------------------------------------------------------ */
    [Header("Dependencies")]
    [Tooltip("Reference to the UnitManager for DI into resources/buildings.")]
    [SerializeField] private UnitManager _unitManager = null;
    [SerializeField] private ResourceManager _resourceManager = null;

    [Header("Grid Manager")]
    [SerializeField] private GridManager _gridManager = null;

    /* ------------------------------------------------------------------ */
    /*  Castle Spawn                                                      */
    /* ------------------------------------------------------------------ */
    [Header("Castle Spawn")]
    [SerializeField] private bool _spawnCastle = true;
    [Tooltip("BuildingData for the main Castle. BuildingPrefab will be instantiated; BuildingModel ignored.")]
    [SerializeField] private BuildingDataSO _castleData = null;
    [Tooltip("Optional explicit bottom-left grid index. Use (-1,-1) to auto-center.")]
    [SerializeField] private Vector2Int _castleGridPos = new Vector2Int(-1, -1);

    /* ------------------------------------------------------------------ */
    /*  Banner Spawn (existing)                                           */
    /* ------------------------------------------------------------------ */
    [Header("Banner Spawn")]
    [SerializeField] private bool _spawnBanner = true;
    [Tooltip("Banner prefab with Banner component.")]
    [SerializeField] private GameObject _bannerPrefab = null;
    [Tooltip("Grid coordinates to place the banner (-1 = auto-center).")]
    [SerializeField] private Vector2Int _bannerGridPos = new Vector2Int(-1, -1);

    /* ------------------------------------------------------------------ */
    /*  Terrain Types & Prefabs                                           */
    /* ------------------------------------------------------------------ */
    [Header("Terrain Types")]
    [SerializeField] private TerrainType _forestTerrainType = null;
    [SerializeField] private TerrainType _rockTerrainType = null;

    [Header("Tree Prefabs")]
    [Tooltip("List of EnvTree prefabs; each prefab must have EnvTree component with Width/Height and Initialize(UnitManager).")]
    [SerializeField] private GameObject[] _treePrefabs = null;

    [Header("Rock Prefabs")]
    [Tooltip("List of EnvRock prefabs; each prefab must have EnvRock component with Width/Height and Initialize(UnitManager).")]
    [SerializeField] private GameObject[] _rockPrefabs = null;

    /* ------------------------------------------------------------------ */
    /*  Spawn Toggles & Probabilities                                     */
    /* ------------------------------------------------------------------ */
    [Header("Spawn Toggles & Probabilities")]
    [SerializeField] private bool _enableTreeSpawning = true;
    [Range(0f, 1f)]
    [SerializeField] private float _treeSpawnProbability = 0.5f;
    [SerializeField] private bool _enableRockSpawning = true;
    [Range(0f, 1f)]
    [SerializeField] private float _rockSpawnProbability = 0.5f;

    /* ------------------------------------------------------------------ */
    /*  Buffer Settings                                                   */
    /* ------------------------------------------------------------------ */
    [Header("Buffer Settings")]
    [Tooltip("Minimum empty cells required around each spawned instance.")]
    [SerializeField] private int _bufferDistance = 2;

    /* ------------------------------------------------------------------ */
    /*  Hierarchy Parent                                                  */
    /* ------------------------------------------------------------------ */
    [Header("Spawn Parent")]
    [SerializeField] private Transform _environmentRoot = null;

    /* ------------------------------------------------------------------ */
    /*  Cached                                                             */
    /* ------------------------------------------------------------------ */
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

    /// <summary>
    /// Entry point called by GridManager after grid is built.
    /// Castle (if enabled) spawns first so later environment respects blocked cells.
    /// </summary>
    public void InitializeEnvironment()
    {
        if (_spawnCastle) SpawnCastle();      // << NEW
        SpawnEnvironment();
        if (_spawnBanner) SpawnBanner();
    }

    /* ================================================================== */
    /*  Castle                                                            */
    /* ================================================================== */

    /// <summary>
    /// Spawn the main Castle building at grid center (or overridden index),
    /// mark its footprint non-walkable, and register with building systems.
    /// </summary>
    private void SpawnCastle()
    {
        if (_castleData == null)
        {
            Debug.LogWarning("EnvironmentSpawner: _spawnCastle enabled but _castleData not assigned.");
            return;
        }

        // Footprint (no rotation override; keep prefab rotation)
        Vector2Int footprint = new Vector2Int(_castleData.SizeX, _castleData.SizeZ);

        int sizeX = _gridManager.GridSettings.GridSizeX;
        int sizeY = _gridManager.GridSettings.GridSizeY;

        // Determine bottom-left grid index
        int gx = _castleGridPos.x >= 0 ? _castleGridPos.x : Mathf.Max(0, (sizeX - footprint.x) / 2);
        int gy = _castleGridPos.y >= 0 ? _castleGridPos.y : Mathf.Max(0, (sizeY - footprint.y) / 2);

        // Validate area
        if (!IsAreaWalkable(new Vector2Int(gx, gy), footprint))
        {
            Debug.LogWarning($"EnvironmentSpawner: Castle area ({gx},{gy}) size {footprint} not fully walkable. Forcing placement and overriding grid.");
        }

        // Instantiate with prefab's default rotation
        GameObject castleGO = Instantiate(_castleData.BuildingPrefab);
        BuildingBase castleBase = castleGO.GetComponent<BuildingBase>();
        if (castleBase == null)
        {
            Debug.LogError("EnvironmentSpawner: Castle prefab missing BuildingBase.");
            Destroy(castleGO);
            return;
        }

        // Basic setup
        castleBase.buildingData = _castleData;
        castleBase.team = Team.Player;
        castleBase.ApplyTeamMaterial();

        // Resource DI (Castle is BuildingResource but may produce none)
        if (castleBase is BuildingResource br && _resourceManager != null)
        {
            br.Initialize(_resourceManager);
        }

        // Snap to grid
        Vector3 snapPos = CalculateSnapPosition(new Vector2Int(gx, gy), footprint);
        castleGO.transform.position = snapPos;
        // Keep prefab rotation (do not override)

        // Occupy grid
        MarkAreaOccupied(new Vector2Int(gx, gy), footprint, false);

        // Register with building system (grid + unit manager)
        castleBase.SetupPlacement(_gridManager, new Vector2Int(gx, gy), footprint, _unitManager);
    }

    /* ================================================================== */
    /*  Banner (existing)                                                 */
    /* ================================================================== */

    private void SpawnBanner()
    {
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

    /* ================================================================== */
    /*  Environment (existing)                                            */
    /* ================================================================== */

    /// <summary>
    /// Iterate grid nodes, randomly spawn trees/rocks with buffer enforcement,
    /// and inject UnitManager into each spawned EnvTree/EnvRock.
    /// Skips any node already marked non-walkable (e.g., Castle footprint).
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
                if (!node.walkable) continue; // << NEW: respect Castle/Banner/other occupied cells

                bool spawnTree = _enableTreeSpawning && node.terrainType == _forestTerrainType
                                 && _treePrefabs.Length > 0 && Random.value <= _treeSpawnProbability;

                bool spawnRock = !spawnTree && _enableRockSpawning && node.terrainType == _rockTerrainType
                                 && _rockPrefabs.Length > 0 && Random.value <= _rockSpawnProbability;

                if (!spawnTree && !spawnRock) continue;

                GameObject prefab = spawnTree
                    ? _treePrefabs[Random.Range(0, _treePrefabs.Length)]
                    : _rockPrefabs[Random.Range(0, _rockPrefabs.Length)];

                int w = 1, h = 1;
                if (spawnTree && prefab.TryGetComponent<EnvTree>(out var t))
                {
                    w = Mathf.Max(1, t.Width);
                    h = Mathf.Max(1, t.Height);
                }
                else if (spawnRock && prefab.TryGetComponent<EnvRock>(out var r))
                {
                    w = Mathf.Max(1, r.Width);
                    h = Mathf.Max(1, r.Height);
                }

                // Clamp footprint to grid
                if (x + w > sizeX) w = sizeX - x;
                if (y + h > sizeY) h = sizeY - y;

                if (HasBufferedNeighbor(occupied, x, y, w, h, _bufferDistance))
                    continue;

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
                _gridManager.SetWalkable(startX + dx, startY + dy, false);
                occupied[startX + dx, startY + dy] = true;
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

    /* ================================================================== */
    /*  Shared Utility (Castle)                                           */
    /* ================================================================== */

    /// <summary>
    /// Equivalent to BuildingPlacementManager.IsAreaWalkable().
    /// </summary>
    private bool IsAreaWalkable(Vector2Int idx, Vector2Int fp)
    {
        for (int x = 0; x < fp.x; x++)
            for (int y = 0; y < fp.y; y++)
            {
                var node = _gridManager.GetNode(idx.x + x, idx.y + y);
                if (node == null || !node.walkable) return false;
            }
        return true;
    }

    /// <summary>
    /// Equivalent to BuildingPlacementManager.MarkAreaOccupied().
    /// </summary>
    private void MarkAreaOccupied(Vector2Int idx, Vector2Int fp, bool walkable)
    {
        for (int x = 0; x < fp.x; x++)
            for (int y = 0; y < fp.y; y++)
                _gridManager.SetWalkable(idx.x + x, idx.y + y, walkable);
    }

    /// <summary>
    /// Equivalent to BuildingPlacementManager.CalculateSnapPosition() without rotation override
    /// (Castle uses prefab rotation).
    /// </summary>
    private Vector3 CalculateSnapPosition(Vector2Int idx, Vector2Int fp)
    {
        float sz = _gridManager.GridSettings.NodeSize;
        // Use world position from bottom-left node to pick Y
        var node = _gridManager.GetNode(idx.x, idx.y);
        float y = node != null ? node.worldPosition.y : 0f;

        float w = fp.x * sz;
        float d = fp.y * sz;

        return new Vector3(
            idx.x * sz + (w - sz) * 0.5f,
            y,
            idx.y * sz + (d - sz) * 0.5f
        );
    }
}
