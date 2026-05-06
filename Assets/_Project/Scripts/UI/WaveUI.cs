// WaveUI.cs
// Painel de controlo de vagas — botão "Iniciar Vaga", countdown e estado da vaga.
// Complementa o UIManager como componente dedicado ao ciclo de vagas.
// Autor: Grupo Iron Bastion | ISTEC Lisboa | UC Gamificação 2024/25
// Data: Abril 2025

using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Componente de UI dedicado ao controlo do ciclo de vagas.
/// Gere o botão "Iniciar Vaga", o texto de countdown e o estado atual da vaga.
/// Subscreve GameEvents para se atualizar automaticamente.
/// Pode coexistir com o UIManager numa cena com canvases separados.
/// </summary>
public class WaveUI : MonoBehaviour
{
    // ─── CONFIGURAÇÃO ────────────────────────────────────────────────────────

    [Header("Botão de Controlo")]
    [SerializeField]
    [Tooltip("Botão que o jogador prime para iniciar a próxima vaga.")]
    private Button startWaveButton;

    [SerializeField]
    [Tooltip("Texto do botão (ex: 'INICIAR VAGA').")]
    private TextMeshProUGUI startWaveButtonText;

    [Header("Textos de Estado")]
    [SerializeField]
    [Tooltip("Texto de countdown entre vagas (ex: 'Próxima vaga em 10s').")]
    private TextMeshProUGUI countdownText;

    [SerializeField]
    [Tooltip("Texto de estado geral da vaga (ex: 'Vaga 1 de 5 — Em curso!').")]
    private TextMeshProUGUI waveStatusText;

    [Header("Painel de Composição (opcional)")]
    [SerializeField]
    [Tooltip("Painel que mostra a composição da próxima vaga (pode ser null).")]
    private GameObject waveCompositionPanel;

    // ─── ESTADO ──────────────────────────────────────────────────────────────

    private int _lastWaveNumber;
    private int _totalWaves;

    // ─── UNITY ───────────────────────────────────────────────────────────────

    private void OnEnable()
    {
        GameEvents.OnWaveStarted      += OnWaveStarted;
        GameEvents.OnWaveCompleted    += OnWaveCompleted;
        GameEvents.OnAllWavesCompleted += OnAllWavesCompleted;
        GameEvents.OnWaveCountdown    += OnWaveCountdown;
        GameEvents.OnGameOver         += OnGameOver;
    }

    private void OnDisable()
    {
        GameEvents.OnWaveStarted      -= OnWaveStarted;
        GameEvents.OnWaveCompleted    -= OnWaveCompleted;
        GameEvents.OnAllWavesCompleted -= OnAllWavesCompleted;
        GameEvents.OnWaveCountdown    -= OnWaveCountdown;
        GameEvents.OnGameOver         -= OnGameOver;
    }

    private void Start()
    {
        // Configurar botão
        startWaveButton?.onClick.AddListener(OnClickStartWave);

        // Estado inicial — fase de preparação
        SetStartButtonInteractable(true);
        SetCountdownVisible(false);

        if (waveStatusText != null)
        {
            int total = WaveManager.Instance != null ? WaveManager.Instance.TotalWaves : 0;
            waveStatusText.text = total > 0
                ? $"Fase de Preparação — {total} vagas"
                : "Fase de Preparação";
        }

        // Esconder painel de composição até ser chamado
        waveCompositionPanel?.SetActive(false);
    }

    // ─── HANDLERS DE EVENTOS ─────────────────────────────────────────────────

    /// <summary>Vaga iniciada — desativar botão e mostrar estado.</summary>
    private void OnWaveStarted(int current, int total)
    {
        _lastWaveNumber = current;
        _totalWaves     = total;

        SetStartButtonInteractable(false);
        SetCountdownVisible(false);
        waveCompositionPanel?.SetActive(false);

        if (waveStatusText != null)
            waveStatusText.text = $"VAGA {current} / {total}  —  Em curso!";
    }

    /// <summary>Vaga concluída — reativar botão ou mostrar mensagem de bónus perfeita.</summary>
    private void OnWaveCompleted(bool isPerfect)
    {
        if (waveStatusText != null)
        {
            if (isPerfect)
                waveStatusText.text = $"Vaga {_lastWaveNumber} — Perfeita! \u2605 +Bónus Ouro";
            else
                waveStatusText.text = $"Vaga {_lastWaveNumber} concluída.";
        }

        // O botão será reativado no fim do countdown (OnWaveCountdown com 0s)
        // ou imediatamente se não houver próxima vaga
    }

    /// <summary>Countdown entre vagas — mostrar temporizador e reativar botão ao fim.</summary>
    private void OnWaveCountdown(float secondsRemaining)
    {
        if (secondsRemaining > 0f)
        {
            SetCountdownVisible(true);
            SetStartButtonInteractable(false);

            if (countdownText != null)
                countdownText.text = $"Próxima vaga em  {Mathf.CeilToInt(secondsRemaining)}s";
        }
        else
        {
            // Countdown terminou — permitir ao jogador iniciar a vaga
            SetCountdownVisible(false);
            SetStartButtonInteractable(true);

            if (waveStatusText != null)
            {
                int next = _lastWaveNumber + 1;
                waveStatusText.text = $"Vaga {next} / {_totalWaves}  —  Pronta!";
            }
        }
    }

    /// <summary>Todas as vagas concluídas — esconder controlos de vaga.</summary>
    private void OnAllWavesCompleted()
    {
        SetStartButtonInteractable(false);
        SetCountdownVisible(false);

        if (startWaveButton != null)
            startWaveButton.gameObject.SetActive(false);

        if (waveStatusText != null)
            waveStatusText.text = "Todas as vagas concluídas!";
    }

    /// <summary>Game Over — desativar controlos.</summary>
    private void OnGameOver(int waveNumber)
    {
        SetStartButtonInteractable(false);
        SetCountdownVisible(false);

        if (waveStatusText != null)
            waveStatusText.text = $"Derrota na Vaga {waveNumber}";
    }

    // ─── HANDLER DO BOTÃO ────────────────────────────────────────────────────

    /// <summary>Iniciar vaga ao clicar no botão.</summary>
    private void OnClickStartWave()
    {
        WaveManager.Instance?.StartWave();
    }

    // ─── UTILITÁRIOS ─────────────────────────────────────────────────────────

    private void SetStartButtonInteractable(bool interactable)
    {
        if (startWaveButton != null)
            startWaveButton.interactable = interactable;
    }

    private void SetCountdownVisible(bool visible)
    {
        if (countdownText != null)
            countdownText.gameObject.SetActive(visible);
    }
}
