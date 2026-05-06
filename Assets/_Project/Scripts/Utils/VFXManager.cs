// VFXManager.cs
// Central manager for all runtime visual effects (particle systems).
// Provides a static API so any system can spawn effects without direct references.
// Uses the ObjectPool for high-frequency effects (impacts, deaths).

using UnityEngine;

/// <summary>
/// Types of VFX available in the game.
/// </summary>
public enum VFXType
{
    ImpactBasic,    // Arrow / bullet hit — small sparks
    ImpactSplash,   // Cannon explosion — dust + debris
    ImpactMagic,    // Arcane bolt — purple burst
    EnemyDeath,     // Enemy death poof
    TowerPlace,     // Tower placed — quick shimmer
}

/// <summary>
/// Singleton VFX manager.  Assign Particle System prefabs in the Inspector;
/// if a slot is empty the effect is silently skipped (graceful degradation).
///
/// Scene setup: add an empty GameObject "VFXManager", attach this script,
/// and assign particle prefabs (one per VFXType).
/// </summary>
public class VFXManager : MonoBehaviour
{
    // ─── SINGLETON ───────────────────────────────────────────────────────────

    public static VFXManager Instance { get; private set; }

    // ─── INSPECTOR ───────────────────────────────────────────────────────────

    [Header("Impact Effects")]
    [SerializeField] private GameObject vfxImpactBasic;
    [SerializeField] private GameObject vfxImpactSplash;
    [SerializeField] private GameObject vfxImpactMagic;

    [Header("Other Effects")]
    [SerializeField] private GameObject vfxEnemyDeath;
    [SerializeField] private GameObject vfxTowerPlace;

    [Header("Pool Settings")]
    [SerializeField] private int prewarmCount = 5;

    // ─── UNITY ───────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        PrewarmPools();
    }

    private void OnEnable()
    {
        GameEvents.OnTowerPlaced += OnTowerPlaced;
    }

    private void OnDisable()
    {
        GameEvents.OnTowerPlaced -= OnTowerPlaced;
    }

    // ─── STATIC API ──────────────────────────────────────────────────────────

    /// <summary>Spawns a VFX at the given world position.</summary>
    public static void Spawn(VFXType type, Vector3 position)
        => Instance?.SpawnInternal(type, position, Quaternion.identity);

    /// <summary>Spawns a VFX with a specific rotation (e.g. aligned to hit normal).</summary>
    public static void Spawn(VFXType type, Vector3 position, Quaternion rotation)
        => Instance?.SpawnInternal(type, position, rotation);

    // ─── INTERNAL SPAWN ──────────────────────────────────────────────────────

    private void SpawnInternal(VFXType type, Vector3 position, Quaternion rotation)
    {
        GameObject prefab = GetPrefab(type);
        if (prefab == null) return;

        GameObject obj;
        if (ObjectPool.Instance != null)
        {
            obj = ObjectPool.Instance.Get(prefab, position, rotation);
        }
        else
        {
            obj = Instantiate(prefab, position, rotation);
        }

        // Auto-return to pool once the particle system finishes
        ParticleSystem ps = obj.GetComponent<ParticleSystem>();
        if (ps != null)
            StartCoroutine(ReturnAfterDuration(obj, ps.main.duration + ps.main.startLifetime.constantMax));
        else
            StartCoroutine(ReturnAfterDuration(obj, 2f));
    }

    // ─── HELPERS ─────────────────────────────────────────────────────────────

    private GameObject GetPrefab(VFXType type) => type switch
    {
        VFXType.ImpactBasic  => vfxImpactBasic,
        VFXType.ImpactSplash => vfxImpactSplash,
        VFXType.ImpactMagic  => vfxImpactMagic,
        VFXType.EnemyDeath   => vfxEnemyDeath,
        VFXType.TowerPlace   => vfxTowerPlace,
        _                    => null,
    };

    private void PrewarmPools()
    {
        if (ObjectPool.Instance == null) return;
        TryPrewarm(vfxImpactBasic);
        TryPrewarm(vfxImpactSplash);
        TryPrewarm(vfxImpactMagic);
        TryPrewarm(vfxEnemyDeath);
        TryPrewarm(vfxTowerPlace);
    }

    private void TryPrewarm(GameObject prefab)
    {
        if (prefab != null)
            ObjectPool.Instance.Prewarm(prefab, prewarmCount);
    }

    private System.Collections.IEnumerator ReturnAfterDuration(GameObject obj, float delay)
    {
        yield return new UnityEngine.WaitForSeconds(delay);
        if (obj != null)
        {
            if (ObjectPool.Instance != null)
                ObjectPool.Instance.Return(obj);
            else
                Destroy(obj);
        }
    }

    // ─── EVENT HANDLERS ──────────────────────────────────────────────────────

    private void OnTowerPlaced(TowerBase tower)
    {
        if (tower != null)
            Spawn(VFXType.TowerPlace, tower.transform.position);
    }
}
