// TowerCannon.cs
// Torre Canhão — dispara projéteis de área (splash) com dano pesado.
// Custo: 150 | Upgrade: 75 | Dano: 80->110 | Splash: 1.2->1.8u | Alcance: 2.5
// Autor: Grupo Iron Bastion | ISTEC Lisboa | UC Gamificação 2024/25
// Data: Abril 2025

using UnityEngine;

/// <summary>
/// Torre Canhão — dano pesado em área. Essencial contra Tanques e grupos.
/// Usa ProjectileSplash para dano em OverlapSphere 3D.
/// Sinergia com Gelo: inimigos abrandados recebem +40% dano.
/// </summary>
public class TowerCannon : TowerBase
{
    protected override void OnInitialized()
    {
        CheckSynergies();
    }

    protected override void OnUpgraded()
    {
        CheckSynergies();
    }

    /// <summary>
    /// Dispara um projétil splash com o raio correto para o nível atual.
    /// </summary>
    protected override void Fire()
    {
        if (_currentTarget == null || towerData == null) return;
        if (towerData.projectilePrefab == null) return;

        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position + Vector3.up * 0.5f;

        GameObject projObj = ObjectPool.Instance != null
            ? ObjectPool.Instance.Get(towerData.projectilePrefab, spawnPos, Quaternion.identity)
            : Instantiate(towerData.projectilePrefab, spawnPos, Quaternion.identity);

        ProjectileSplash splash = projObj.GetComponent<ProjectileSplash>();
        if (splash != null)
        {
            float radius = towerData.GetSplashRadius(_level);
            splash.InitializeSplash(_currentTarget, towerData.GetDamage(_level), radius, _damageBonusMultiplier);
        }
        else
        {
            // Fallback para projétil base se não tiver ProjectileSplash
            ProjectileBase proj = projObj.GetComponent<ProjectileBase>();
            proj?.Initialize(_currentTarget, towerData.GetDamage(_level), _damageBonusMultiplier);
        }
    }

    /// <summary>
    /// Verifica sinergia com Torres de Gelo adjacentes.
    /// Gelo + Canhão = Canhão causa +40% dano a inimigos abrandados.
    /// </summary>
    private void CheckSynergies()
    {
        Collider[] cols = Physics.OverlapSphere(transform.position, 1.5f, LayerMask.GetMask("Tower"));

        foreach (Collider col in cols)
        {
            if (col.gameObject == gameObject) continue;

            TowerBase neighbor = col.GetComponentInParent<TowerBase>();
            if (neighbor is TowerIce)
            {
                // +40% dano a inimigos abrandados
                // Nota: o multiplicador é verificado no ProjectileSplash contra inimigos com slow ativo.
                // Por simplificação, aplicamos o bónus de dano global quando a sinergia está ativa.
                ApplyDamageBonus(1.4f);
            }
        }
    }
}
