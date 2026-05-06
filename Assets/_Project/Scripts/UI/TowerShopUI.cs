// TowerShopUI.cs
// Painel de compra de torres — mostra botões de compra e gere o modo de colocação.
// Autor: Grupo Iron Bastion | ISTEC Lisboa | UC Gamificação 2024/25
// Data: Abril 2025

using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Gere o painel de loja de torres.
/// Cada botão de compra está ligado a um TowerData via Inspector.
/// </summary>
public class TowerShopUI : MonoBehaviour
{
    // ─── CONFIGURAÇÃO ────────────────────────────────────────────────────────

    [System.Serializable]
    public struct TowerButton
    {
        [Tooltip("Dados da torre associada a este botão.")]
        public TowerData towerData;
        [Tooltip("Botão de compra na UI.")]
        public Button button;
        [Tooltip("Label com o custo da torre.")]
        public TextMeshProUGUI costLabel;
        [Tooltip("Label com o nome da torre.")]
        public TextMeshProUGUI nameLabel;
        [Tooltip("Imagem do ícone da torre.")]
        public Image iconImage;
    }

    [SerializeField]
    [Tooltip("Lista de botões de torres disponíveis neste nível.")]
    private TowerButton[] towerButtons;

    // ─── UNITY ───────────────────────────────────────────────────────────────

    private void Start()
    {
        // Configurar cada botão
        foreach (TowerButton tb in towerButtons)
        {
            if (tb.towerData == null || tb.button == null) continue;

            TowerData data = tb.towerData; // Capturar para o lambda

            // Preencher labels
            if (tb.nameLabel != null) tb.nameLabel.text = data.towerName;
            if (tb.costLabel != null) tb.costLabel.text = $"{data.cost}";
            if (tb.iconImage != null && data.icon != null) tb.iconImage.sprite = data.icon;

            // Registar callback de clique
            tb.button.onClick.AddListener(() => OnClickBuyTower(data));
        }

        // Subscrever ao evento de mudança de ouro para atualizar disponibilidade
        GameEvents.OnGoldChanged += OnGoldChanged;
        GameEvents.OnWaveStarted += OnWaveStarted;
        GameEvents.OnWaveCompleted += OnWaveCompleted;

        UpdateButtonStates();
    }

    private void OnDestroy()
    {
        GameEvents.OnGoldChanged  -= OnGoldChanged;
        GameEvents.OnWaveStarted  -= OnWaveStarted;
        GameEvents.OnWaveCompleted -= OnWaveCompleted;
    }

    // ─── HANDLERS ────────────────────────────────────────────────────────────

    private void OnClickBuyTower(TowerData data)
    {
        if (GameManager.Instance == null) return;
        if (!GameManager.Instance.CanAfford(data.cost)) return;
        if (WaveManager.Instance != null && WaveManager.Instance.IsWaveActive) return;

        GridManager.Instance?.BeginPlacement(data);
    }

    private void OnGoldChanged(int gold)
    {
        UpdateButtonStates();
    }

    private void OnWaveStarted(int wave, int total)
    {
        // Desativar botões de compra durante vaga ativa
        SetButtonsInteractable(false);
    }

    private void OnWaveCompleted(bool isPerfect)
    {
        // Reativar botões após vaga
        UpdateButtonStates();
    }

    // ─── ATUALIZAÇÃO DE ESTADO ───────────────────────────────────────────────

    /// <summary>Atualiza a interatividade de cada botão conforme o ouro disponível.</summary>
    private void UpdateButtonStates()
    {
        if (GameManager.Instance == null) return;

        bool waveActive = WaveManager.Instance != null && WaveManager.Instance.IsWaveActive;

        foreach (TowerButton tb in towerButtons)
        {
            if (tb.button == null || tb.towerData == null) continue;

            bool canAfford = GameManager.Instance.CanAfford(tb.towerData.cost);
            tb.button.interactable = canAfford && !waveActive;
        }
    }

    private void SetButtonsInteractable(bool interactable)
    {
        foreach (TowerButton tb in towerButtons)
            if (tb.button != null)
                tb.button.interactable = interactable;
    }
}
