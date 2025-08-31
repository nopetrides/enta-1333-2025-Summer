using UnityEngine;

[CreateAssetMenu(fileName = "EnemyWave", menuName = "Game/EnemyWave")]
public class EnemyWaveSO : ScriptableObject
{
    public ArmyComposition[] enemies;
    public float spawnDelay = 0.5f;
}

