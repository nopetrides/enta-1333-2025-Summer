using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generic object pool for MonoBehaviours.
/// Pre-warms the configured prefabs, then grows on demand.
/// </summary>
public class ObjectPool : MonoBehaviour
{
    // ---------- Singleton ----------
    public static ObjectPool Instance { get; private set; }

    // ---------- Inspector ----------
    [Header("Prefabs & Pre-warm")]
    [SerializeField] private ArrowProjectile _arrowPrefab;
    [SerializeField] private MagicImpactEffect _magicPrefab;
    [Tooltip("Objects created per prefab during Awake().")]
    [SerializeField] private int _initialSize = 32;

    // ---------- Internal state ----------
    private readonly Dictionary<System.Type, Queue<MonoBehaviour>> _pools = new();

    // ================= Lifecycle =================
    private void Awake()
    {
        // Singleton safety
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Pre-warm the prefabs configured in Inspector
        InitPool(_arrowPrefab);
        InitPool(_magicPrefab);
    }

    // ================= Public API =================
    /// <summary>Returns a pooled ArrowProjectile.</summary>
    public ArrowProjectile RentArrow() => Rent(_arrowPrefab);

    /// <summary>Returns a pooled MagicImpactEffect.</summary>
    public MagicImpactEffect RentMagic() => Rent(_magicPrefab);

    /// <summary>Generic return that fits Action&lt;Projectile&gt; delegate.</summary>
    public void ReturnToPool(MonoBehaviour obj) => Return(obj);

    // Convenience overloads (no boxing / cast)
    public void ReturnToPool(ArrowProjectile arrow) => Return(arrow);
    public void ReturnToPool(MagicImpactEffect impact) => Return(impact);

    // ================= Core Pool Logic =================
    private void InitPool<T>(T prefab) where T : MonoBehaviour
    {
        if (prefab == null) return;

        Queue<MonoBehaviour> queue = GetQueue(prefab.GetType());
        Prewarm(prefab, queue, _initialSize);
    }

    private T Rent<T>(T prefab) where T : MonoBehaviour
    {
        if (prefab == null) return null;

        Queue<MonoBehaviour> queue = GetQueue(prefab.GetType());
        T obj = queue.Count > 0 ? (T)queue.Dequeue()
                                : Instantiate(prefab);

        return obj;
    }

    private void Return(MonoBehaviour obj)
    {
        if (obj == null) return;

        Queue<MonoBehaviour> queue = GetQueue(obj.GetType());
        obj.gameObject.SetActive(false);
        queue.Enqueue(obj);
    }

    // ---------- Helpers ----------
    private Queue<MonoBehaviour> GetQueue(System.Type type)
    {
        if (!_pools.TryGetValue(type, out var queue))
        {
            queue = new Queue<MonoBehaviour>();
            _pools[type] = queue;
        }
        return queue;
    }

    private void Prewarm<T>(T prefab, Queue<MonoBehaviour> queue, int amount)
        where T : MonoBehaviour
    {
        for (int i = 0; i < amount; i++)
        {
            T obj = Instantiate(prefab, transform);
            obj.gameObject.SetActive(false);
            queue.Enqueue(obj);
        }
    }
}
