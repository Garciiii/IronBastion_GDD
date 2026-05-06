// TowerInfoPanel.cs
// Painel de informação e upgrade/venda da torre selecionada.
// Autor: Grupo Iron Bastion | ISTEC Lisboa | UC Gamificação 2024/25
// Data: Abril 2025

using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Painel que aparece ao clicar numa torre existente.
/// Mostra stats, custo de upgrade e valor de venda.
/// </summary>
public class TowerInfoPanel : MonoBehaviour
{
    // ─── CONFIGURAÇÃO ────────────────────────────────────────────────────────

    [Header("Labels de Informação")]
    [SerializeField] private TextMeshProUGUI textTowerName;
    [SerializeField] private TextMeshProUGUI textDamage;
    [SerializeField] private TextMeshProUGUI textRange;
    [SerializeField] private TextMeshProUGUI textLevel;
    [SerializeField] private TextMeshProUGUI textUpgradeCost;
    [SerializeField] private TextMeshProUGUI textSellValue;

    [Header("Botões")]
    [SerializeField] private Button buttonUpgrade;
    [SerializeField] private Button buttonSell;

    [Header("Targeting")]
    [SerializeField] private Button buttonTargetFirst;
    [SerializeField] private Button buttonTargetLast;
    [SerializeField] private Button buttonTargetStrongest;
    [SerializeField] private Button buttonTargetWeakest;

    // ─── ESTADO ──────────────────────────────────────────────────────────────

    private TowerBase _selectedTower;

    // ─── UNITY ───────────────────────────────────────────────────────────────

    private void Start()
    {
        // Subscrever eventos de seleção
        GameEvents.OnTowerSelected   += ShowPanel;
        GameEvents.OnTowerDeselected += HidePanel;
        GameEvents.OnTowerSold       += OnTowerSoldOrUpgraded;
        GameEvents.OnTowerUpgraded   += OnTowerSoldOrUpgraded;

        // Configurar botões
        buttonUpgrade?.onClick.AddListener(OnClickUpgrade);
        buttonSell?.onClick.AddListener(OnClickSell);
        buttonTargetFirst?.onClick.AddListener(() => SetTargeting(TargetingMode.First));
        buttonTargetLast?.onClick.AddListener(() => SetTargeting(TargetingMode.Last));
        buttonTargetStrongest?.onClick.AddListener(() => SetTargeting(TargetingMode.Strongest));
        buttonTargetWeakest?.onClick.AddListener(() => SetTargeting(TargetingMode.Weakest));

        // Começar oculto
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        GameEvents.OnTowerSelected   -= ShowPanel;
        GameEvents.OnTowerDeselected -= HidePanel;
        GameEvents.OnTowerSold       -= OnTowerSoldOrUpgraded;
        GameEvents.OnTowerUpgraded   -= OnTowerSoldOrUpgraded;
    }

    // ─── MOSTRAR / OCULTAR ───────────────────────────────────────────────────

    private void ShowPanel(TowerBase tower)
    {
        _selectedTower = tower;
        gameObject.SetActive(true);
        RefreshPanel();
    }

    private void HidePanel()
    {
        _selectedTower = null;
        gameObject.SetActive(false);
    }

    private void OnTowerSoldOrUpgraded(TowerBase tower)
    {
        if (tower == _selectedTower)
        {
            if (tower == null || !tower.gameObject.activeInHierarchy)
                HidePanel();
            else
                RefreshPanel();
        }
    }

    // ─── REFRESH ─────────────────────────────────────────────────────────────

    private void RefreshPanel()
    {
        if (_selectedTower == null || _selectedTower.Data == null) return;

        TowerData data = _selectedTower.Data;
        int level = _selectedTower.Level;

        if (textTowerName != null)
            textTowerName.text = $"{data.towerName} Nv.{level}";

        if (textDamage != null)
        {
            float dmg = data.GetDamage(level);
            float dmgNext = data.GetDamage(2);
            textDamage.text = _selectedTower.IsMaxLevel
                ? $"Dano: {dmg}"
                : $"Dano: {dmg} → {dmgNext}";
        }

        if (textRange != null)
            textRange.text = $"Alcance: {data.GetRange(level):F1}u";

        if (textLevel != null)
            textLevel.text = _selectedTower.IsMaxLevel ? "NÍVEL MÁXIMO" : $"Nível {level}";

        if (textUpgradeCost != null)
            textUpgradeCost.text = _selectedTower.IsMaxLevel ? "—" : $"{data.upgradeCost}";

        if (textSellValue != null)
            textSellValue.text = $"{_selectedTower.SellValue}";

        // Botão upgrade só ativo se não estiver no máximo e tiver ouro
        if (buttonUpgrade != null)
        {
            bool canUpgrade = !_selectedTower.IsMaxLevel
                && GameManager.Instance != null
                && GameManager.Instance.CanAfford(data.upgradeCost);
            buttonUpgrade.interactable = canUpgrade;
        }
    }

    // ─── HANDLERS DE BOTÕES ──────────────────────────────────────────────────

    private void OnClickUpgrade()
    {
        if (_selectedTower == null) return;
        bool success = _selectedTower.Upgrade();
        if (success) RefreshPanel();
    }

    private void OnClickSell()
    {
        if (_selectedTower == null) return;
        _selectedTower.Sell();
        HidePanel();
    }

    private void SetTargeting(TargetingMode mode)
    {
        if (_selectedTower == null) return;
        _selectedTower.Targeting = mode;
    }
}
