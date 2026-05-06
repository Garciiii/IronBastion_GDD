// TowerBase.cs
// Classe base para todas as torres de Iron Bastion.
// Gere targeting 3D, disparo, upgrade, venda e sinergias.
// Autor: Grupo Iron Bastion | ISTEC Lisboa | UC Gamificação 2024/25
// Data: Abril 2025

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Modos de targeting disponíveis para cada torre.
/// Alterável individualmente pelo jogador no painel de upgrade.
/// </summary>
public enum TargetingMode
{
    /// <summary>Inimigo mais avançado no caminho.</summary>
    First,
    /// <summary>Inimigo mais atrasado no caminho.</summary>
    Last,
    /// <summary>Inimigo com mais HP atual.</summary>
    Strongest,
    /// <summary>Inimigo com menos HP atual.</summary>
    Weakest
}

/// <summary>
/// Classe base que todas as torres herdam.
/// Implementa o loop de targeting 3D, disparo com cooldown e sistema de upgrade/venda.
/// </summary>
public class TowerBase : MonoBehaviour
{
    // ─── CONFIGURAÇÃO ────────────────────────────────────────────────────────

    [Header("Dados da Torre")]
    [SerializeField]
    [Tooltip("ScriptableObject com os dados desta torre.")]
    protected TowerData towerData;

    [Header("Pontos de Disparo")]
    [SerializeField]
    [Tooltip("Ponto 3D de onde os projéteis são disparados.")]
    protected Transform firePoint;

    [Header("Targeting")]
    [SerializeField]
    [Tooltip("Modo de targeting inicial desta torre.")]
    private TargetingMode targetingMode = TargetingMode.First;

    [Header("Sinergia — Visual")]
    [SerializeField]
    [Tooltip("LineRenderer para mostrar ligações de sinergia no editor.")]
    private LineRenderer synergyLine;

    // ─── ESTADO ──────────────────────────────────────────────────────────────

    protected int _level = 1;
    protected float _fireTimer;
    protected EnemyBase _currentTarget;
    protected GridCell _occupiedCell;
    protected int _totalInvested;   // Custo base + upgrades — para calcular valor de venda

    // Bónus de sinergia acumulados
    protected float _rangeBonusMultiplier  = 1f;   // ex: 1.2 para +20% alcance (Arcana)
    protected float _damageBonusMultiplier = 1f;   // ex: 1.4 para +40% dano (Canhão + Gelo)
    protected float _fireRateBonusMultiplier = 1f; // ex: 1.1 para +10% cadência (Arqueira + Arqueira)

    // ─── PROPRIEDADES PÚBLICAS ───────────────────────────────────────────────

    /// <summary>Nível atual da torre (1 ou 2).</summary>
    public int Level => _level;

    /// <summary>Está no nível máximo?</summary>
    public bool IsMaxLevel => _level >= 2;

    /// <summary>Custo de upgrade para nível seguinte.</summary>
    public int UpgradeCost => towerData != null ? towerData.upgradeCost : 0;

    /// <summary>Valor de venda (50% do total investido).</summary>
    public int SellValue => Mathf.RoundToInt(_totalInvested * 0.5f);

    /// <summary>Nome desta torre.</summary>
    public string TowerName => towerData != null ? towerData.towerName : "Torre";

    /// <summary>Dados do ScriptableObject.</summary>
    public TowerData Data => towerData;

    /// <summary>Modo de targeting ativo.</summary>
    public TargetingMode Targeting
    {
        get => targetingMode;
        set => targetingMode = value;
    }

    /// <summary>Alcance efetivo (com bónus de sinergia).</summary>
    public float EffectiveRange => towerData != null
        ? towerData.GetRange(_level) * _rangeBonusMultiplier
        : 3f;

    // ─── UNITY ───────────────────────────────────────────────────────────────

    protected virtual void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.IsGameOver) return;
        if (!WaveManager.Instance.IsWaveActive) return;

        FindTarget();
        HandleFiring();
        RotateTowardsTarget();
    }

    // ─── INICIALIZAÇÃO ───────────────────────────────────────────────────────

    /// <summary>
    /// Inicializa a torre com dados e célula ocupada. Chamado pelo GridManager após colocação.
    /// </summary>
    public virtual void Initialize(TowerData data, GridCell cell)
    {
        towerData      = data;
        _occupiedCell  = cell;
        _level         = 1;
        _totalInvested = data.cost;
        _fireTimer     = 0f;
        targetingMode  = data.defaultTargeting;

        // Pré-aquecer pool do projétil
        if (data.projectilePrefab != null && ObjectPool.Instance != null)
            ObjectPool.Instance.Prewarm(data.projectilePrefab, 5);

        OnInitialized();
    }

    /// <summary>Ponto de extensão para subclasses após inicialização.</summary>
    protected virtual void OnInitialized() { }

    // ─── TARGETING ───────────────────────────────────────────────────────────

    /// <summary>Encontra o alvo prioritário dentro do alcance.</summary>
    protected virtual void FindTarget()
    {
        // Detetar inimigos no raio de alcance via OverlapSphere 3D
        Collider[] cols = Physics.OverlapSphere(
            transform.position,
            EffectiveRange,
            LayerMask.GetMask("Enemy")
        );

        if (cols.Length == 0)
        {
            _currentTarget = null;
            return;
        }

        // Construir lista de candidatos válidos
        List<EnemyBase> candidates = new();
        foreach (Collider col in cols)
        {
            EnemyBase e = col.GetComponentInParent<EnemyBase>();
            if (e != null && e.gameObject.activeInHierarchy)
                candidates.Add(e);
        }

        if (candidates.Count == 0)
        {
            _currentTarget = null;
            return;
        }

        _currentTarget = SelectTarget(candidates);
    }

    private EnemyBase SelectTarget(List<EnemyBase> candidates)
    {
        switch (targetingMode)
        {
            case TargetingMode.First:
                // Menor distância restante até ao fim = mais avançado
                EnemyBase first = candidates[0];
                foreach (var e in candidates)
                    if (e.DistanceToEnd < first.DistanceToEnd)
                        first = e;
                return first;

            case TargetingMode.Last:
                EnemyBase last = candidates[0];
                foreach (var e in candidates)
                    if (e.DistanceToEnd > last.DistanceToEnd)
                        last = e;
                return last;

            case TargetingMode.Strongest:
                EnemyBase strongest = candidates[0];
                foreach (var e in candidates)
                    if (e.CurrentHealth > strongest.CurrentHealth)
                        strongest = e;
                return strongest;

            case TargetingMode.Weakest:
                EnemyBase weakest = candidates[0];
                foreach (var e in candidates)
                    if (e.CurrentHealth < weakest.CurrentHealth)
                        weakest = e;
                return weakest;

            default:
                return candidates[0];
        }
    }

    // ─── DISPARO ─────────────────────────────────────────────────────────────

    protected virtual void HandleFiring()
    {
        if (_currentTarget == null) return;

        _fireTimer -= Time.deltaTime;
        if (_fireTimer > 0f) return;

        float fireRate = towerData.GetFireRate(_level) * _fireRateBonusMultiplier;
        _fireTimer = 1f / fireRate;

        Fire();
    }

    /// <summary>Dispara um projétil. Subclasses podem sobrescrever para comportamento especial.</summary>
    protected virtual void Fire()
    {
        if (towerData.projectilePrefab == null || _currentTarget == null) return;

        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position + Vector3.up * 0.5f;
        GameObject projObj = ObjectPool.Instance != null
            ? ObjectPool.Instance.Get(towerData.projectilePrefab, spawnPos, Quaternion.identity)
            : Instantiate(towerData.projectilePrefab, spawnPos, Quaternion.identity);

        ProjectileBase proj = projObj.GetComponent<ProjectileBase>();
        if (proj != null)
            proj.Initialize(_currentTarget, towerData.GetDamage(_level), _damageBonusMultiplier);
    }

    // ─── ROTAÇÃO ─────────────────────────────────────────────────────────────

    private void RotateTowardsTarget()
    {
        if (_currentTarget == null) return;
        Vector3 dir = (_currentTarget.transform.position - transform.position);
        dir.y = 0f;
        if (dir == Vector3.zero) return;
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(dir),
            Time.deltaTime * 10f
        );
    }

    // ─── UPGRADE ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Melhora a torre para o nível 2. Debita o custo do GameManager.
    /// </summary>
    public virtual bool Upgrade()
    {
        if (IsMaxLevel) return false;
        if (GameManager.Instance == null) return false;
        if (!GameManager.Instance.CanAfford(towerData.upgradeCost)) return false;

        GameManager.Instance.SpendGold(towerData.upgradeCost);
        _totalInvested += towerData.upgradeCost;
        _level = 2;

        OnUpgraded();
        GameEvents.OnTowerUpgraded?.Invoke(this);
        return true;
    }

    /// <summary>Ponto de extensão para subclasses após upgrade.</summary>
    protected virtual void OnUpgraded() { }

    // ─── VENDA ───────────────────────────────────────────────────────────────

    /// <summary>Vende a torre e devolve 50% do investimento ao jogador.</summary>
    public virtual void Sell()
    {
        GameManager.Instance?.AddGold(SellValue);
        _occupiedCell?.RemoveTower();
        GameEvents.OnTowerSold?.Invoke(this);
        Destroy(gameObject);
    }

    // ─── SINERGIAS ───────────────────────────────────────────────────────────

    /// <summary>
    /// Aplica um bónus de alcance por sinergia (ex: Torre Arcana).
    /// </summary>
    public virtual void ApplyRangeBonus(float multiplier)
    {
        _rangeBonusMultiplier = Mathf.Max(_rangeBonusMultiplier, multiplier);
    }

    /// <summary>Aplica um bónus de dano por sinergia (ex: Canhão + Gelo).</summary>
    public virtual void ApplyDamageBonus(float multiplier)
    {
        _damageBonusMultiplier = Mathf.Max(_damageBonusMultiplier, multiplier);
    }

    /// <summary>Aplica um bónus de cadência de fogo por sinergia (ex: Arqueira + Arqueira).</summary>
    public virtual void ApplyFireRateBonus(float multiplier)
    {
        _fireRateBonusMultiplier = Mathf.Max(_fireRateBonusMultiplier, multiplier);
    }

    /// <summary>Remove todos os bónus de sinergia (chamado ao vender uma torre vizinha).</summary>
    public virtual void ClearSynergyBonuses()
    {
        _rangeBonusMultiplier    = 1f;
        _damageBonusMultiplier   = 1f;
        _fireRateBonusMultiplier = 1f;
    }

    // ─── GIZMOS ──────────────────────────────────────────────────────────────

    private void OnDrawGizmosSelected()
    {
        if (towerData == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, EffectiveRange);
    }
}
