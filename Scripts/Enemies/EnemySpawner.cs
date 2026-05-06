// EnemySpawner.cs
// Responsável por instanciar inimigos no ponto de spawn usando o ObjectPool.
// Recebe instruções do WaveManager sobre o que e quando spawnar.
// Autor: Grupo Iron Bastion | ISTEC Lisboa | UC Gamificação 2024/25
// Data: Abril 2025

using System.Collections;
using UnityEngine;

/// <summary>
/// Singleton que spawna inimigos no início do caminho 3D.
/// Usa o ObjectPool para evitar Instantiate/Destroy frequentes.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    // ─── SINGLETON ───────────────────────────────────────────────────────────

    public static EnemySpawner Instance { get; private set; }

    // ─── CONFIGURAÇÃO ────────────────────────────────────────────────────────

    [SerializeField]
    [Tooltip("Transform do ponto de spawn (WP0 — fora do ecrã).")]
    private Transform spawnPoint;

    [SerializeField]
    [Tooltip("Quantidade inicial de cada prefab a pré-instanciar no pool.")]
    private int poolPrewarmCount = 10;

    // ─── ESTADO ──────────────────────────────────────────────────────────────

    private bool _isSpawning;

    // ─── UNITY ───────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Posicionar o spawn point no WP0 se não estiver definido
        if (spawnPoint == null)
            spawnPoint = transform;
    }

    // ─── API PÚBLICA ─────────────────────────────────────────────────────────

    /// <summary>
    /// Pré-aquece o pool para um conjunto de WaveDatas.
    /// Deve ser chamado pelo WaveManager no início do nível.
    /// </summary>
    public void PrewarmForWaves(WaveData[] waves)
    {
        if (ObjectPool.Instance == null) return;

        foreach (WaveData wave in waves)
        {
            if (wave == null || wave.enemyGroups == null) continue;

            foreach (EnemyGroup group in wave.enemyGroups)
            {
                if (group.enemyData?.prefab != null)
                    ObjectPool.Instance.Prewarm(group.enemyData.prefab, poolPrewarmCount);
            }
        }
    }

    /// <summary>
    /// Spawna os inimigos de uma vaga de forma assíncrona.
    /// Os inimigos são spawned um a um com o intervalo definido no WaveData.
    /// </summary>
    public Coroutine SpawnWave(WaveData waveData, System.Action onWaveSpawnComplete)
    {
        return StartCoroutine(SpawnWaveRoutine(waveData, onWaveSpawnComplete));
    }

    // ─── ROTINA DE SPAWN ─────────────────────────────────────────────────────

    private IEnumerator SpawnWaveRoutine(WaveData waveData, System.Action onComplete)
    {
        _isSpawning = true;

        if (waveData.enemyGroups != null)
        {
            foreach (EnemyGroup group in waveData.enemyGroups)
            {
                if (group.enemyData == null || group.enemyData.prefab == null) continue;

                for (int i = 0; i < group.count; i++)
                {
                    SpawnEnemy(group.enemyData, group.healthOverride);
                    yield return new WaitForSeconds(waveData.spawnInterval);
                }
            }
        }

        _isSpawning = false;
        onComplete?.Invoke();
    }

    private void SpawnEnemy(EnemyData enemyData, float healthOverride)
    {
        if (ObjectPool.Instance == null) return;

        // Obter inimigo do pool
        GameObject enemyObj = ObjectPool.Instance.Get(
            enemyData.prefab,
            spawnPoint.position,
            Quaternion.identity
        );

        // Inicializar o componente EnemyBase
        EnemyBase enemy = enemyObj.GetComponent<EnemyBase>();
        if (enemy != null)
            enemy.Initialize(enemyData, healthOverride);
        else
            Debug.LogWarning($"[EnemySpawner] Prefab '{enemyData.prefab.name}' não tem componente EnemyBase.");
    }
}
