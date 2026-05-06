// ProjectileSplash.cs
// Projétil do Canhão — ao atingir o alvo, causa dano em área (OverlapSphere 3D).
// Autor: Grupo Iron Bastion | ISTEC Lisboa | UC Gamificação 2024/25
// Data: Abril 2025

using UnityEngine;

/// <summary>
/// Projétil com dano em área usado pela Torre Canhão.
/// Ao atingir o alvo principal, causa dano a todos os inimigos dentro do splash radius.
/// </summary>
public class ProjectileSplash : ProjectileBase
{
    // ─── CONFIGURAÇÃO ────────────────────────────────────────────────────────

    [SerializeField]
    [Tooltip("Raio de explosão em unidades Unity. Configurado via TowerData.")]
    private float splashRadius = 1.2f;

    [SerializeField]
    [Tooltip("Layer mask dos inimigos para o OverlapSphere.")]
    private LayerMask enemyLayerMask;

    [SerializeField]
    [Tooltip("Prefab de efeito visual de explosão (opcional).")]
    private GameObject explosionEffect;

    // ─── API PÚBLICA ─────────────────────────────────────────────────────────

    /// <summary>
    /// Inicializa o projétil splash com raio de área adicional.
    /// </summary>
    public void InitializeSplash(EnemyBase target, float dmg, float radius, float dmgMultiplier = 1f)
    {
        base.Initialize(target, dmg, dmgMultiplier);
        splashRadius = radius;
    }

    // ─── IMPACTO ─────────────────────────────────────────────────────────────

    protected override void OnHit()
    {
        Vector3 hitPosition = transform.position;

        // Efeito visual de explosão (se configurado)
        if (explosionEffect != null)
            Instantiate(explosionEffect, hitPosition, Quaternion.identity);

        // Detectar todos os inimigos no raio de splash com OverlapSphere 3D
        Collider[] hitColliders = Physics.OverlapSphere(hitPosition, splashRadius, enemyLayerMask);

        foreach (Collider col in hitColliders)
        {
            EnemyBase enemy = col.GetComponentInParent<EnemyBase>();
            if (enemy != null)
                enemy.TakeDamage(damage * _damageMultiplier);
        }

        ReturnToPool();
    }

    protected override void SpawnImpactVFX(Vector3 position)
        => VFXManager.Spawn(VFXType.ImpactSplash, position);

    // ─── GIZMOS ──────────────────────────────────────────────────────────────

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        Gizmos.DrawSphere(transform.position, splashRadius);
    }
}
