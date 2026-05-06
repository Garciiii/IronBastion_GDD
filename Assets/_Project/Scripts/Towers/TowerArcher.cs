// TowerArcher.cs
// Torre Arqueira — dispara 1 projétil no nível 1, 2 alvos simultâneos no nível 2.
// Custo: 75 | Upgrade: 38 | Dano: 15->22 | Alcance: 3.0 | Cadência: 1.5->2.1/s
// Autor: Grupo Iron Bastion | ISTEC Lisboa | UC Gamificação 2024/25
// Data: Abril 2025

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Torre Arqueira — torre base e mais eficiente em custo.
/// Nível 2: dispara simultaneamente para 2 alvos diferentes.
/// Sinergia recebida: +20% alcance se Torre Arcana adjacente.
/// Sinergia fornecida: +10% cadência a Arqueiras adjacentes.
/// </summary>
public class TowerArcher : TowerBase
{
    [Header("Arqueira — Configuração Especial")]
    [SerializeField]
    [Tooltip("Segundo ponto de disparo para o nível 2 (alvo secundário).")]
    private Transform firePoint2;

    protected override void OnInitialized()
    {
        // Verificar sinergias com torres adjacentes ao ser colocada
        CheckSynergies();
    }

    protected override void OnUpgraded()
    {
        CheckSynergies();
    }

    /// <summary>
    /// No nível 2, a Torre Arqueira dispara para o alvo principal E para um segundo alvo.
    /// </summary>
    protected override void Fire()
    {
        if (_currentTarget == null || towerData == null) return;

        // Disparo principal (sempre)
        FireAt(_currentTarget, firePoint != null ? firePoint.position : transform.position + Vector3.up * 0.5f);

        // Nível 2: encontrar e disparar para segundo alvo diferente do principal
        if (_level >= 2)
        {
            EnemyBase secondTarget = FindSecondTarget();
            if (secondTarget != null)
            {
                Vector3 pos2 = firePoint2 != null
                    ? firePoint2.position
                    : transform.position + Vector3.up * 0.5f + Vector3.right * 0.3f;
                FireAt(secondTarget, pos2);
            }
        }
    }

    private void FireAt(EnemyBase target, Vector3 spawnPos)
    {
        if (towerData.projectilePrefab == null) return;

        GameObject projObj = ObjectPool.Instance != null
            ? ObjectPool.Instance.Get(towerData.projectilePrefab, spawnPos, Quaternion.identity)
            : Instantiate(towerData.projectilePrefab, spawnPos, Quaternion.identity);

        ProjectileBase proj = projObj.GetComponent<ProjectileBase>();
        if (proj != null)
            proj.Initialize(target, towerData.GetDamage(_level), _damageBonusMultiplier);
    }

    /// <summary>Encontra um segundo alvo diferente do principal dentro do alcance.</summary>
    private EnemyBase FindSecondTarget()
    {
        Collider[] cols = Physics.OverlapSphere(transform.position, EffectiveRange, LayerMask.GetMask("Enemy"));

        foreach (Collider col in cols)
        {
            EnemyBase e = col.GetComponentInParent<EnemyBase>();
            if (e != null && e != _currentTarget && e.gameObject.activeInHierarchy)
                return e;
        }
        return null;
    }

    /// <summary>
    /// Verifica sinergias com torres vizinhas:
    /// - Recebe +20% alcance de Torres Arcana adjacentes.
    /// - Dá e recebe +10% cadência com Arqueiras adjacentes.
    /// </summary>
    private void CheckSynergies()
    {
        Collider[] cols = Physics.OverlapSphere(transform.position, 1.5f, LayerMask.GetMask("Tower"));

        foreach (Collider col in cols)
        {
            if (col.gameObject == gameObject) continue;

            TowerBase neighbor = col.GetComponentInParent<TowerBase>();
            if (neighbor == null) continue;

            // Arqueira + Arqueira = +10% cadência para ambas
            if (neighbor is TowerArcher archerNeighbor)
            {
                ApplyFireRateBonus(1.1f);
                archerNeighbor.ApplyFireRateBonus(1.1f);
            }

            // Arcana adjacente = +20% alcance para esta torre
            if (neighbor is TowerArcane)
                ApplyRangeBonus(1.2f);
        }
    }
}
