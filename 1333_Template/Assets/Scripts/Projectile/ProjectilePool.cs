using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple object pool that handles different projectile prefabs
/// in separate queues.
/// </summary>
public class ProjectilePool : MonoBehaviour
{
    public static ProjectilePool Instance { get; private set; }

    [Header("Prefabs & Pre-warm")]
    [SerializeField] private ArrowProjectile _arrowPrefab;
    [SerializeField] private MagicProjectile _magicPrefab;
    [SerializeField] private int _initialSize = 16;

    private readonly Queue<ArrowProjectile> _arrows = new();
    private readonly Queue<MagicProjectile> _magic = new();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        Prewarm(_arrowPrefab, _arrows);
        Prewarm(_magicPrefab, _magic);
    }

    /// <summary>Returns a pooled projectile matching the requested type.</summary>
    public Projectile Rent(AttackType type)
    {
        return type switch
        {
            AttackType.Ranged => Rent(_arrowPrefab, _arrows),
            AttackType.Magic => Rent(_magicPrefab, _magic),
            _ => null
        };
    }

    /// <summary>Re-queues the projectile.</summary>
    public void ReturnToPool(Projectile proj)
    {
        proj.gameObject.SetActive(false);

        if (proj is ArrowProjectile arrow) _arrows.Enqueue(arrow);
        else if (proj is MagicProjectile magic) _magic.Enqueue(magic);
    }

    // ---------- Internal helpers ----------
    private static T Rent<T>(T prefab, Queue<T> queue) where T : Projectile
    {
        T proj = queue.Count > 0 ? queue.Dequeue()
                                 : Object.Instantiate(prefab);
        return proj;
    }

    private void Prewarm<T>(T prefab, Queue<T> queue) where T : Projectile
    {
        for (int i = 0; i < _initialSize; i++)
        {
            T proj = Instantiate(prefab, transform);
            proj.gameObject.SetActive(false);
            queue.Enqueue(proj);
        }
    }
}
