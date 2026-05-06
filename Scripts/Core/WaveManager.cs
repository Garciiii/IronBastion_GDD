// WaveManager.cs
// Gere a progressão de vagas: countdown, spawning, deteção de fim de vaga.
// Autor: Grupo Iron Bastion | ISTEC Lisboa | UC Gamificação 2024/25
// Data: Abril 2025

using System.Collections;
using UnityEngine;

/// <summary>
/// Estados possíveis do WaveManager.
/// </summary>
public enum WaveState
{
    /// <summary>Fase de preparação — o jogador pode colocar torres.</summary>
    Preparation,
    /// <summary>Inimigos a ser spawned.</summary>
    Spawning,
    /// <summary>Inimigos em campo, vaga ativa.</summary>
    WaveActive,
    /// <summary>Todos os inimigos eliminados ou chegaram à base; countdown para próxima vaga.</summary>
    Countdown,
    /// <summary>Todas as vagas completadas — vitória.</summary>
    Completed
}

/// <summary>
/// Singleton que coordena a progressão de vagas do nível.
/// Comunica com EnemySpawner para spawnar inimigos e usa GameEvents para notificar a UI.
/// </summary>
public class WaveManager : MonoBehaviour
{
    // ─── SINGLETON ───────────────────────────────────────────────────────────

    public static WaveManager Instance { get; private set; }

    // ─── CONFIGURAÇÃO ────────────────────────────────────────────────────────

    [Header("Vagas do Nível")]
    [SerializeField]
    [Tooltip("Array com os ScriptableObjects WaveData para este nível, pela ordem de aparição.")]
    private WaveData[] waves;

    [Header("Timing")]
    [SerializeField]
    [Tooltip("Tempo de countdown em segundos entre o fim de uma vaga e o início da seguinte.")]
    private float timeBetweenWaves = 15f;

    // ─── ESTADO ──────────────────────────────────────────────────────────────

    private int _currentWaveIndex;
    private WaveState _state;
    private int _enemiesAliveInWave;
    private int _enemiesReachedBaseInWave;
    private int _waveGoldEarned;
    private Coroutine _countdownCoroutine;

    // ─── PROPRIEDADES PÚBLICAS ───────────────────────────────────────────────

    /// <summary>Número da vaga atual (1-based).</summary>
    public int CurrentWave => _currentWaveIndex + 1;

    /// <summary>Total de vagas neste nível.</summary>
    public int TotalWaves => waves != null ? waves.Length : 0;

    /// <summary>Estado atual do WaveManager.</summary>
    public WaveState State => _state;

    /// <summary>Vagas ativas (spawning ou inimigos em campo)?</summary>
    public bool IsWaveActive => _state == WaveState.Spawning || _state == WaveState.WaveActive;

    // ─── UNITY ───────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // Subscrever eventos de inimigos
        GameEvents.OnEnemyKilled    += HandleEnemyKilled;
        GameEvents.OnEnemyReachedEnd += HandleEnemyReachedEnd;

        // Pré-aquecer o pool para todas as vagas
        if (EnemySpawner.Instance != null && waves != null)
            EnemySpawner.Instance.PrewarmForWaves(waves);

        // Ir para fase de preparação
        SetState(WaveState.Preparation);
    }

    private void OnDestroy()
    {
        GameEvents.OnEnemyKilled     -= HandleEnemyKilled;
        GameEvents.OnEnemyReachedEnd -= HandleEnemyReachedEnd;
    }

    // ─── API PÚBLICA ─────────────────────────────────────────────────────────

    /// <summary>
    /// Inicia a vaga atual. Chamado pelo botão "Iniciar Vaga" na UI ou pela tecla Space.
    /// </summary>
    public void StartWave()
    {
        if (_state != WaveState.Preparation) return;
        if (_currentWaveIndex >= TotalWaves) return;

        WaveData currentWave = waves[_currentWaveIndex];
        _enemiesAliveInWave = currentWave.GetTotalEnemyCount();
        _enemiesReachedBaseInWave = 0;
        _waveGoldEarned = 0;

        // Cancelar countdown se estava a decorrer
        if (_countdownCoroutine != null)
        {
            StopCoroutine(_countdownCoroutine);
            _countdownCoroutine = null;
        }

        SetState(WaveState.Spawning);
        GameEvents.OnWaveStarted?.Invoke(CurrentWave, TotalWaves);

        // Iniciar spawning
        EnemySpawner.Instance?.SpawnWave(currentWave, OnAllEnemiesSpawned);
    }

    // ─── HANDLERS DE EVENTOS ─────────────────────────────────────────────────

    private void HandleEnemyKilled(int goldReward)
    {
        _enemiesAliveInWave--;
        _waveGoldEarned += goldReward;
        CheckWaveEnd();
    }

    private void HandleEnemyReachedEnd(EnemyBase enemy)
    {
        _enemiesAliveInWave--;
        _enemiesReachedBaseInWave++;
        CheckWaveEnd();
    }

    private void OnAllEnemiesSpawned()
    {
        if (_state == WaveState.Spawning)
            SetState(WaveState.WaveActive);
    }

    // ─── LÓGICA DE VAGAS ─────────────────────────────────────────────────────

    private void CheckWaveEnd()
    {
        if (_enemiesAliveInWave > 0) return;
        if (_state != WaveState.Spawning && _state != WaveState.WaveActive) return;

        // Vaga terminada
        bool isPerfect = _enemiesReachedBaseInWave == 0;

        // Bónus de vaga perfeita: +20% do ouro gerado na vaga
        if (isPerfect)
        {
            int bonus = Mathf.RoundToInt(_waveGoldEarned * 0.2f);
            GameManager.Instance?.AddGold(bonus);
        }

        GameEvents.OnWaveCompleted?.Invoke(isPerfect);

        _currentWaveIndex++;

        if (_currentWaveIndex >= TotalWaves)
        {
            // Todas as vagas concluídas
            SetState(WaveState.Completed);
            GameEvents.OnAllWavesCompleted?.Invoke();
        }
        else
        {
            // Iniciar countdown para próxima vaga
            SetState(WaveState.Countdown);
            _countdownCoroutine = StartCoroutine(CountdownRoutine());
        }
    }

    private IEnumerator CountdownRoutine()
    {
        float remaining = timeBetweenWaves;
        while (remaining > 0f)
        {
            GameEvents.OnWaveCountdown?.Invoke(remaining);
            yield return new WaitForSeconds(1f);
            remaining -= 1f;
        }
        GameEvents.OnWaveCountdown?.Invoke(0f);
        SetState(WaveState.Preparation);
    }

    // ─── UTILITÁRIOS ─────────────────────────────────────────────────────────

    private void SetState(WaveState newState)
    {
        _state = newState;
    }

    private void Update()
    {
        // Atalho de teclado para iniciar vaga
        if (Input.GetKeyDown(KeyCode.Space) && _state == WaveState.Preparation)
            StartWave();
    }
}
