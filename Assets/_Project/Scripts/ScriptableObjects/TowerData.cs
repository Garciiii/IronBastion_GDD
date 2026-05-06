// TowerData.cs
// ScriptableObject com todos os dados configuráveis de uma torre.
// Permite ajustar balanceamento diretamente no Inspector sem alterar código.
// Autor: Grupo Iron Bastion | ISTEC Lisboa | UC Gamificação 2024/25
// Data: Abril 2025

using UnityEngine;

/// <summary>
/// Asset de dados de uma torre. Cria um asset por tipo de torre em Data/Towers/.
/// </summary>
[CreateAssetMenu(fileName = "TowerData_New", menuName = "IronBastion/Tower Data")]
public class TowerData : ScriptableObject
{
    [Header("Identificação")]
    [Tooltip("Nome visível ao jogador.")]
    public string towerName = "Torre";

    [Tooltip("Descrição de lore para tooltip.")]
    [TextArea(2, 4)]
    public string description = "";

    [Tooltip("Prefab 3D desta torre.")]
    public GameObject prefab;

    [Tooltip("Ícone para o painel de loja.")]
    public Sprite icon;

    // ─── CUSTO ──────────────────────────────────────────────────────────────

    [Header("Custo")]
    [Tooltip("Custo de compra em ouro.")]
    public int cost = 100;

    [Tooltip("Custo de upgrade para nível 2.")]
    public int upgradeCost = 50;

    // ─── ESTATÍSTICAS — NÍVEL 1 ─────────────────────────────────────────────

    [Header("Estatísticas — Nível 1")]
    [Tooltip("Dano por projétil no nível 1. 0 para torres de suporte (Gelo).")]
    public float damageLevel1 = 20f;

    [Tooltip("Alcance de deteção de inimigos em unidades Unity (nível 1).")]
    public float rangeLevel1 = 3f;

    [Tooltip("Disparos por segundo no nível 1. 0 para torres sem projéteis.")]
    public float fireRateLevel1 = 1f;

    // ─── ESTATÍSTICAS — NÍVEL 2 ─────────────────────────────────────────────

    [Header("Estatísticas — Nível 2")]
    [Tooltip("Dano por projétil no nível 2.")]
    public float damageLevel2 = 30f;

    [Tooltip("Alcance no nível 2.")]
    public float rangeLevel2 = 3f;

    [Tooltip("Disparos por segundo no nível 2.")]
    public float fireRateLevel2 = 1.5f;

    // ─── DADOS ESPECIAIS ────────────────────────────────────────────────────

    [Header("Dados Especiais (Canhão)")]
    [Tooltip("Raio de explosão splash no nível 1. 0 para torres sem splash.")]
    public float splashRadiusLevel1 = 0f;

    [Tooltip("Raio de explosão splash no nível 2.")]
    public float splashRadiusLevel2 = 0f;

    [Header("Dados Especiais (Gelo)")]
    [Tooltip("Percentagem de slow aplicado (0.5 = 50%). Nível 1.")]
    [Range(0f, 1f)]
    public float slowPercentLevel1 = 0f;

    [Tooltip("Percentagem de slow aplicado (0.7 = 70%). Nível 2.")]
    [Range(0f, 1f)]
    public float slowPercentLevel2 = 0f;

    [Tooltip("Duração do slow em segundos por pulso.")]
    public float slowDuration = 0.7f;

    [Tooltip("Intervalo entre pulsos de slow em segundos.")]
    public float slowPulseInterval = 0.5f;

    [Header("Dados Especiais (Arcana)")]
    [Tooltip("Dano por segundo do DoT no nível 1.")]
    public float dotDamagePerSecLevel1 = 0f;

    [Tooltip("Dano por segundo do DoT no nível 2.")]
    public float dotDamagePerSecLevel2 = 0f;

    [Tooltip("Duração do DoT em segundos no nível 1.")]
    public float dotDurationLevel1 = 0f;

    [Tooltip("Duração do DoT em segundos no nível 2.")]
    public float dotDurationLevel2 = 0f;

    [Tooltip("Raio de amplificação de torres vizinhas.")]
    public float synergyRadius = 2.5f;

    // ─── TARGETING ──────────────────────────────────────────────────────────

    [Header("Targeting")]
    [Tooltip("Modo de targeting padrão desta torre.")]
    public TargetingMode defaultTargeting = TargetingMode.First;

    // ─── PREFAB DE PROJÉTIL ─────────────────────────────────────────────────

    [Header("Projétil")]
    [Tooltip("Prefab do projétil disparado. Null para torres sem projéteis (Gelo).")]
    public GameObject projectilePrefab;

    [Tooltip("Velocidade do projétil em unidades/segundo.")]
    public float projectileSpeed = 8f;

    // ─── MÉTODOS UTILITÁRIOS ────────────────────────────────────────────────

    /// <summary>Retorna o dano correspondente ao nível dado.</summary>
    public float GetDamage(int level) => level >= 2 ? damageLevel2 : damageLevel1;

    /// <summary>Retorna o alcance correspondente ao nível dado.</summary>
    public float GetRange(int level) => level >= 2 ? rangeLevel2 : rangeLevel1;

    /// <summary>Retorna a cadência correspondente ao nível dado.</summary>
    public float GetFireRate(int level) => level >= 2 ? fireRateLevel2 : fireRateLevel1;

    /// <summary>Retorna o splash radius correspondente ao nível dado.</summary>
    public float GetSplashRadius(int level) => level >= 2 ? splashRadiusLevel2 : splashRadiusLevel1;

    /// <summary>Retorna o slow percent correspondente ao nível dado.</summary>
    public float GetSlowPercent(int level) => level >= 2 ? slowPercentLevel2 : slowPercentLevel1;

    /// <summary>Retorna o dano de DoT por segundo correspondente ao nível dado.</summary>
    public float GetDotDamage(int level) => level >= 2 ? dotDamagePerSecLevel2 : dotDamagePerSecLevel1;

    /// <summary>Retorna a duração do DoT correspondente ao nível dado.</summary>
    public float GetDotDuration(int level) => level >= 2 ? dotDurationLevel2 : dotDurationLevel1;
}
