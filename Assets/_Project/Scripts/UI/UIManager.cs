// UIManager.cs
// Gere todo o HUD em jogo: vidas, ouro, vaga, pausa, vitória e derrota.
// Autor: Grupo Iron Bastion | ISTEC Lisboa | UC Gamificação 2024/25
// Data: Abril 2025

using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Singleton que controla todos os elementos de UI em jogo.
/// Subscreve os GameEvents e atualiza os elementos visuais em resposta.
/// </summary>
public class UIManager : MonoBehaviour
{
    // ─── SINGLETON ───────────────────────────────────────────────────────────

    public static UIManager Instance { get; private set; }

    // ─── HUD ─────────────────────────────────────────────────────────────────

    [Header("HUD — Estado do Jogo")]
    [SerializeField] private TextMeshProUGUI textGold;
    [SerializeField] private TextMeshProUGUI textLives;
    [SerializeField] private TextMeshProUGUI textWave;
    [SerializeField] private TextMeshProUGUI textCountdown;

    [Header("HUD — Botões")]
    [SerializeField] private Button buttonStartWave;
    [SerializeField] private Button buttonPause;

    // ─── PAINÉIS ─────────────────────────────────────────────────────────────

    [Header("Painéis")]
    [SerializeField] private GameObject panelPause;
    [SerializeField] private GameObject panelVictory;
    [SerializeField] private GameObject panelGameOver;

    // ─── PAINEL DE PAUSA ─────────────────────────────────────────────────────

    [Header("Pausa")]
    [SerializeField] private Button buttonResume;
    [SerializeField] private Button buttonRestart;
    [SerializeField] private Button buttonMainMenu;

    // ─── PAINEL DE VITÓRIA ───────────────────────────────────────────────────

    [Header("Vitória")]
    [SerializeField] private TextMeshProUGUI textVictoryScore;
    [SerializeField] private TextMeshProUGUI textVictoryLives;
    [SerializeField] private TextMeshProUGUI textVictoryPerfect;
    [SerializeField] private GameObject[] starObjects;   // 3 estrelas — ativar conforme resultado
    [SerializeField] private Button buttonNextLevel;
    [SerializeField] private Button buttonVictoryMenu;

    // ─── PAINEL DE DERROTA ───────────────────────────────────────────────────

    [Header("Derrota")]
    [SerializeField] private TextMeshProUGUI textGameOverWave;
    [SerializeField] private Button buttonRetry;
    [SerializeField] private Button buttonGameOverMenu;

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
        // Subscrever eventos
        GameEvents.OnGoldChanged    += UpdateGold;
        GameEvents.OnLivesChanged   += UpdateLives;
        GameEvents.OnWaveStarted    += UpdateWaveLabel;
        GameEvents.OnWaveCountdown  += UpdateCountdown;
        GameEvents.OnVictory        += ShowVictoryPanel;
        GameEvents.OnGameOver       += ShowGameOverPanel;
        GameEvents.OnGamePaused     += OnGamePaused;
        GameEvents.OnGameResumed    += OnGameResumed;

        // Configurar botões
        buttonStartWave?.onClick.AddListener(OnClickStartWave);
        buttonPause?.onClick.AddListener(OnClickPause);
        buttonResume?.onClick.AddListener(OnClickResume);
        buttonRestart?.onClick.AddListener(OnClickRestart);
        buttonMainMenu?.onClick.AddListener(OnClickMainMenu);
        buttonNextLevel?.onClick.AddListener(OnClickNextLevel);
        buttonVictoryMenu?.onClick.AddListener(OnClickMainMenu);
        buttonRetry?.onClick.AddListener(OnClickRestart);
        buttonGameOverMenu?.onClick.AddListener(OnClickMainMenu);

        // Garantir que painéis de overlay estão fechados no início
        panelPause?.SetActive(false);
        panelVictory?.SetActive(false);
        panelGameOver?.SetActive(false);

        if (textCountdown != null) textCountdown.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        GameEvents.OnGoldChanged    -= UpdateGold;
        GameEvents.OnLivesChanged   -= UpdateLives;
        GameEvents.OnWaveStarted    -= UpdateWaveLabel;
        GameEvents.OnWaveCountdown  -= UpdateCountdown;
        GameEvents.OnVictory        -= ShowVictoryPanel;
        GameEvents.OnGameOver       -= ShowGameOverPanel;
        GameEvents.OnGamePaused     -= OnGamePaused;
        GameEvents.OnGameResumed    -= OnGameResumed;
    }

    private void Update()
    {
        // Escape: pausa durante jogo, fechar painel durante pausa
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (panelPause != null && panelPause.activeSelf)
                OnClickResume();
            else if (GameManager.Instance != null && !GameManager.Instance.IsGameOver)
                OnClickPause();
        }

        // Atualizar visibilidade do botão de iniciar vaga
        if (buttonStartWave != null && WaveManager.Instance != null)
            buttonStartWave.gameObject.SetActive(WaveManager.Instance.State == WaveState.Preparation);
    }

    // ─── HANDLERS DE EVENTOS ─────────────────────────────────────────────────

    private void UpdateGold(int gold)
    {
        if (textGold != null) textGold.text = $"{gold}";
    }

    private void UpdateLives(int lives)
    {
        if (textLives != null) textLives.text = $"{lives}";
    }

    private void UpdateWaveLabel(int current, int total)
    {
        if (textWave != null) textWave.text = $"VAGA {current} / {total}";
        if (textCountdown != null) textCountdown.gameObject.SetActive(false);
    }

    private void UpdateCountdown(float seconds)
    {
        if (textCountdown == null) return;
        if (seconds <= 0f)
        {
            textCountdown.gameObject.SetActive(false);
        }
        else
        {
            textCountdown.gameObject.SetActive(true);
            textCountdown.text = $"Próxima vaga em {Mathf.CeilToInt(seconds)}s";
        }
    }

    private void ShowVictoryPanel(int score, int stars, int perfectWaves)
    {
        panelVictory?.SetActive(true);

        if (textVictoryScore != null)
            textVictoryScore.text = $"Pontuação: {score} pts";

        if (textVictoryLives != null && GameManager.Instance != null)
            textVictoryLives.text = $"Vidas: {GameManager.Instance.Lives}/{GameManager.Instance.StartingLives}";

        if (textVictoryPerfect != null && WaveManager.Instance != null)
            textVictoryPerfect.text = $"Vagas perfeitas: {perfectWaves}/{WaveManager.Instance.TotalWaves}";

        // Ativar estrelas
        if (starObjects != null)
        {
            for (int i = 0; i < starObjects.Length; i++)
                if (starObjects[i] != null)
                    starObjects[i].SetActive(i < stars);
        }
    }

    private void ShowGameOverPanel(int wave)
    {
        panelGameOver?.SetActive(true);
        if (textGameOverWave != null && WaveManager.Instance != null)
            textGameOverWave.text = $"Resististe até à Vaga {wave} de {WaveManager.Instance.TotalWaves}";
    }

    private void OnGamePaused()
    {
        panelPause?.SetActive(true);
    }

    private void OnGameResumed()
    {
        panelPause?.SetActive(false);
    }

    // ─── HANDLERS DE BOTÕES ──────────────────────────────────────────────────

    private void OnClickStartWave()
    {
        WaveManager.Instance?.StartWave();
    }

    private void OnClickPause()
    {
        GameManager.Instance?.Pause();
    }

    private void OnClickResume()
    {
        GameManager.Instance?.Resume();
    }

    private void OnClickRestart()
    {
        GameManager.Instance?.RestartLevel();
    }

    private void OnClickMainMenu()
    {
        GameManager.Instance?.LoadMainMenu();
    }

    private void OnClickNextLevel()
    {
        GameManager.Instance?.LoadNextLevel();
    }
}
