using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaveManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private AStarPathfinder pathfinder;
    [SerializeField] private List<EnemyWaveSO> waves = new();
    [SerializeField] private List<Material> enemyMaterials;
    [SerializeField] private List<Transform> spawnPoints;
    [SerializeField] private GameObject kingTarget;
    [SerializeField] private float timeBetweenWaves = 10f;

    private int currentWave = -1;
    private bool isSpawning = false;
    public event System.Action<int> OnWaveStarted;

    public void Initialize()
    {
        StartCoroutine(WaveLoop());
    }

    IEnumerator WaveLoop()
    {
        yield return new WaitForSeconds(2f);
        while (currentWave + 1 < waves.Count)
        {
            currentWave++;

        
            float t = timeBetweenWaves;      // show countdown timer
            while (t > 0)
            {
                UIManager.Instance?.ShowCountdown(t);
                yield return null;
                t -= Time.deltaTime;
            }
            UIManager.Instance?.HideCountdown();

            isSpawning = true;
            OnWaveStarted?.Invoke(currentWave + 1);
            UIManager.Instance?.UpdateWave(currentWave + 1);
            yield return StartCoroutine(SpawnWave(waves[currentWave]));
            isSpawning = false;
        }
        GameManager.Instance?.OnWin();
    }

    IEnumerator SpawnWave(EnemyWaveSO wave)
    {
        foreach (var army in wave.enemies)
        {
            foreach (var unitEntry in army.units)
            {
                for (int i = 0; i < unitEntry.count; i++)
                {
                    var spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];
                    Vector3 spawnPos = spawnPoint.position;
                    GameObject go = Instantiate(unitEntry.unitTypePrefab.prefab, spawnPos, Quaternion.identity);
                    UnitInstance unit = go.GetComponent<UnitInstance>();
                    Material enemyMat = enemyMaterials.Count > 0 ? enemyMaterials[0] : null;
                    unit.Initialize(pathfinder, enemyMat, 1);

                    Vector2Int gridIdx = gridManager.WorldToGridIndex(spawnPos);
                    gridManager.SetUnitOccupancy(gridIdx.x, gridIdx.y, true, unit);

                   
                    var ai = go.GetComponent<EnemyAI>();   // ensure  enemyai is present and initialized
                    if (!ai) ai = go.AddComponent<EnemyAI>();
                    ai.SetTarget(kingTarget.transform); // always set king as fallback

                    yield return new WaitForSeconds(wave.spawnDelay);
                }
            }
        }
       
    }
}
