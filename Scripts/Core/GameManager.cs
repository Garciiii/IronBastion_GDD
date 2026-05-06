// GameManager.cs
// Singleton central do jogo. Gere o estado global: ouro, vidas, pontuação, pausa.
// Autor: Grupo Iron Bastion | ISTEC Lisboa | UC Gamificação 2024/25
// Data: Abril 2025

using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Singleton que mantém o estado global do jogo.
/// Comunica com outros sistemas exclusivamente através de GameEvents.
/// </summary>
public class GameManager : MonoBehaviour
{
    // ─── SINGLETON ───────────────────────────────────────────────────────────

    public static GameManager Instance { get; private set; }

    // ─── CONFIGURAÇÃO ────────────────────────────────────────────────────────

    [Header("Configuração do Nível")]
    [SerializeField]
    [Tooltip("Ouro inicial para este nível.")]
    private int startingGold = 150;

    [SerializeField]
    [Tooltip("Número de vidas iniciais.")]
    private int startingLives = 10;

    [SerializeField]
    [Tooltip("Índice da cena do próximo nível. -1 se for o último nível.")]
    private int nextLevelSceneIndex = -1;

    // ─── ESTADO DO JOGO ──────────────────────────────────────────────────────

    private int _gold;
    private int _lives;
    private int _score;
    private int _perfectWaves;
    private bool _isPaused;
    private bool _isGameOver;

    // ─── PROPRIEDADES PÚBLICAS ───────────────────────────────────────────────

    /// <summary>Ouro atual do jogador.</summary>
    public int Gold => _gold;

    /// <summary>Vidas atuais do jogador.</summary>
    public int Lives => _lives;

    /// <summary>Pontuação atual.</summary>
    public int Score => _score;

    /// <summary>O jogo está pausado?</summary>
    public bool IsPaused => _isPaused;

    /// <summary>O jogo terminou?</summary>
    public bool IsGameOver => _isGameOver;

    /// <summary>Número de vidas máximas neste nível.</summary>
    public int StartingLives => startingLives;

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
        // Inicializar estado
        _gold = startingGold;
        _lives = startingLives;
        _score = 0;
        _perfectWaves = 0;
        _isPaused = false;
        _isGameOver = false;
        Time.timeScale = 1f;

        // Subscrever eventos relevantes
        GameEvents.OnEnemyKilled += HandleEnemyKilled;
        GameEvents.OnEnemyReachedEnd += HandleEnemyReachedEnd;
        GameEvents.OnWaveCompleted += HandleWaveCompleted;
        GameEvents.OnAllWavesCompleted += HandleAllWavesCompleted;

        // Notificar UI do estado inicial
        GameEvents.OnGoldChanged?.Invoke(_gold);
        GameEvents.OnLivesChanged?.Invoke(_lives);
    }

    private void OnDestroy()
    {
        // Cancelar subscrições para evitar referências nulas
        GameEvents.OnEnemyKilled -= HandleEnemyKilled;
        GameEvents.OnEnemyReachedEnd -= HandleEnemyReachedEnd;
        GameEvents.OnWaveCompleted -= HandleWaveCompleted;
        GameEvents.OnAllWavesCompleted -= HandleAllWavesCompleted;
    }

    // ─── OURO ────────────────────────────────────────────────────────────────

    /// <summary>Adiciona ouro ao jogador e notifica a UI.</summary>
    public void AddGold(int amount)
    {
        _gold += amount;
        GameEvents.OnGoldChanged?.Invoke(_gold);
        GameEvents.OnGoldEarned?.Invoke(amount);
    }

    /// <summary>
    /// Tenta gastar ouro. Retorna true se o jogador tinha ouro suficiente.
    /// </summary>
    public bool SpendGold(int amount)
    {
        if (_gold < amount) return false;
        _gold -= amount;
        GameEvents.OnGoldChanged?.Invoke(_gold);
        return true;
    }

    /// <summary>Verifica se o jogador pode pagar um custo sem efetuar a transação.</summary>
    public bool CanAfford(int amount) => _gold >= amount;

    // ─── VIDAS ───────────────────────────────────────────────────────────────

    /// <summary>Remove vidas quando um inimigo chega à base.</summary>
    public void LoseLives(int amount)
    {
        if (_isGameOver) return;

        _lives = Mathf.Max(0, _lives - amount);
        GameEvents.OnLivesChanged?.Invoke(_lives);
        GameEvents.OnBaseDamaged?.Invoke(amount);

        if (_lives <= 0)
            TriggerGameOver();
    }

    // ─── PONTUAÇÃO ───────────────────────────────────────────────────────────

    /// <summary>Adiciona pontos à pontuação atual.</summary>
    public void AddScore(int points)
    {
        _score += points;
    }

    /// <summary>Calcula e guarda o high score no PlayerPrefs.</summary>
    private void SaveHighScore(int levelIndex)
    {
        string key = $"HighScore_Level{levelIndex}";
        int current = PlayerPrefs.GetInt(key, 0);
        if (_score > current)
        {
            PlayerPrefs.SetInt(key, _score);
            PlayerPrefs.Save();
        }
    }

    // ─── PAUSA ───────────────────────────────────────────────────────────────

    /// <summary>Alterna entre pausa e jogo ativo.</summary>
    public void TogglePause()
    {
        _isPaused = !_isPaused;
        Time.timeScale = _isPaused ? 0f : 1f;

        if (_isPaused)
            GameEvents.OnGamePaused?.Invoke();
        else
            GameEvents.OnGameResumed?.Invoke();
    }

    /// <summary>Pausa o jogo.</summary>
    public void Pause()
    {
        if (_isPaused) return;
        TogglePause();
    }

    /// <summary>Retoma o jogo.</summary>
    public void Resume()
    {
        if (!_isPaused) return;
        TogglePause();
    }

    // ─── ESTADO DO JOGO ──────────────────────────────────────────────────────

    private void TriggerGameOver()
    {
        _isGameOver = true;
        Time.timeScale = 0f;
        int currentWave = WaveManager.Instance != null ? WaveManager.Instance.CurrentWave : 0;
        GameEvents.OnGameOver?.Invoke(currentWave);
    }

    /// <summary>Reinicia o nível atual.</summary>
    public void RestartLevel()
    {
        Time.timeScale = 1f;
        GameEvents.ClearAllEvents();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>Carrega o menu principal.</summary>
    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        GameEvents.ClearAllEvents();
        SceneManager.LoadScene(0);
    }

    /// <summary>Carrega o próximo nível.</summary>
    public void LoadNextLevel()
    {
        if (nextLevelSceneIndex < 0)
        {
            LoadMainMenu();
            return;
        }
        Time.timeScale = 1f;
        GameEvents.ClearAllEvents();
        SceneManager.LoadScene(nextLevelSceneIndex);
    }

    // ─── HANDLERS DE EVENTOS ─────────────────────────────────────────────────

    private void HandleEnemyKilled(int goldReward)
    {
        AddGold(goldReward);
        AddScore(5); // Pequena pontuação por cada inimigo eliminado
    }

    private void HandleEnemyReachedEnd(EnemyBase enemy)
    {
        LoseLives(enemy.LivesLost);
    }

    private void HandleWaveCompleted(bool isPerfect)
    {
        AddScore(100); // Bónus por completar vaga
        if (isPerfect)
        {
            _perfectWaves++;
            AddScore(100); // Bónus de vaga perfeita (total 200 pts)
        }
    }

    private void HandleAllWavesCompleted()
    {
        if (_isGameOver) return;

        // Calcular pontuação final
        int baseScore = 1000;
        int stars = CalculateStars();
        int starBonus = (stars - 1) * 500;
        AddScore(baseScore + starBonus);

        // Guardar high score
        SaveHighScore(SceneManager.GetActiveScene().buildIndex);

        // Disparar evento de vitória
        GameEvents.OnVictory?.Invoke(_score, stars, _perfectWaves);
        Time.timeScale = 0f;
    }

    /// <summary>Calcula o número de estrelas com base nas vidas restantes.</summary>
    public int CalculateStars()
    {
        float ratio = (float)_lives / startingLives;
        if (_lives >= 8) return 3;
        if (_lives >= 4) return 2;
        return 1;
    }
}
