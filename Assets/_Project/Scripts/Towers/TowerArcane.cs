// TowerArcane.cs
// Torre Arcana — dano médio + DoT + amplifica torres vizinhas (+20% alcance).
// Custo: 200 | Upgrade: 100 | Dano: 35->43 | DoT: 5->8 dps | Alcance: 3.5
// Autor: Grupo Iron Bastion | ISTEC Lisboa | UC Gamificação 2024/25
// Data: Abril 2025

using UnityEngine;

/// <summary>
/// Torre Arcana — causa dano com DoT e amplifica torres vizinhas.
/// Sinergia fornecida: torres no raio de 2.5u ganham +20% alcance.
/// Sinergia com Gelo: Torre de Gelo vizinha passa de 50% para 65% slow.
/// Targeting: Strongest (inimigo com mais HP).
/// </summary>
public class TowerArcane : TowerBase
{
    protected override void OnInitialized()
    {
        // A Torre Arcana usa targeting Strongest por padrão
        Targeting = TargetingMode.Strongest;

        // Amplificar torres vizinhas ao ser colocada
        ApplySynergiesToNeighbors();
    }

    protected override void OnUpgraded()
    {
        ApplySynergiesToNeighbors();
    }

    private void OnDestroy()
    {
        // Ao ser destruída/vendida, remover bónus das vizinhas
        RemoveSynergiesFromNeighbors();
    }

    /// <summary>
    /// Dispara um projétil mágico com DoT ao atingir o alvo.
    /// </summary>
    protected override void Fire()
    {
        if (_currentTarget == null || towerData == null) return;
        if (towerData.projectilePrefab == null) return;

        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position + Vector3.up * 0.5f;

        GameObject projObj = ObjectPool.Instance != null
            ? ObjectPool.Instance.Get(towerData.projectilePrefab, spawnPos, Quaternion.identity)
            : Instantiate(towerData.projectilePrefab, spawnPos, Quaternion.identity);

        ProjectileBase proj = projObj.GetComponent<ProjectileBase>();
        if (proj != null)
        {
            float dotDps      = towerData.GetDotDamage(_level);
            float dotDuration = towerData.GetDotDuration(_level);
            proj.Initialize(_currentTarget, towerData.GetDamage(_level), _damageBonusMultiplier, dotDps, dotDuration);
        }
    }

    // ─── SINERGIAS ───────────────────────────────────────────────────────────

    /// <summary>
    /// Aplica o bónus de +20% alcance a todas as torres no raio de sinergia.
    /// Torres de Gelo adjacentes recebem slow upgrade (50% -> 65%).
    /// </summary>
    private void ApplySynergiesToNeighbors()
    {
        if (towerData == null) return;

        Collider[] cols = Physics.OverlapSphere(
            transform.position,
            towerData.synergyRadius,
            LayerMask.GetMask("Tower")
        );

        foreach (Collider col in cols)
        {
            if (col.gameObject == gameObject) continue;

            TowerBase neighbor = col.GetComponentInParent<TowerBase>();
            if (neighbor == null) continue;

            // Todas as torres vizinhas ganham +20% alcance
            neighbor.ApplyRangeBonus(1.2f);

            // Torres de Gelo adjacentes: slow 50% -> 65%
            // (Gerido internamente pela TowerIce via CheckSynergies)
        }
    }

    private void RemoveSynergiesFromNeighbors()
    {
        if (towerData == null) return;

        Collider[] cols = Physics.OverlapSphere(
            transform.position,
            towerData.synergyRadius,
            LayerMask.GetMask("Tower")
        );

        foreach (Collider col in cols)
        {
            if (col.gameObject == gameObject) continue;
            TowerBase neighbor = col.GetComponentInParent<TowerBase>();
            neighbor?.ClearSynergyBonuses();
        }
    }
}
