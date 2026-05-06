// ProjectileDoT.cs
// Projétil especializado da Torre Arcana — dano imediato + DoT (Damage over Time).
// As propriedades de DoT são configuráveis no Inspector como defaults do prefab,
// mas podem ser sobrepostas pelo TowerArcane via Initialize().
// Autor: Grupo Iron Bastion | ISTEC Lisboa | UC Gamificação 2024/25
// Data: Abril 2025

using UnityEngine;

/// <summary>
/// Projétil mágico que aplica dano imediato e DoT (Damage over Time) ao alvo.
/// Usado como prefab de projétil da Torre Arcana.
/// O DoT respeita a imunidade do Corrompido Tanque (gerida na EnemyBase).
/// Herda todo o movimento homing de ProjectileBase.
/// </summary>
public class ProjectileDoT : ProjectileBase
{
    // ─── CONFIGURAÇÃO DoT ────────────────────────────────────────────────────

    [Header("Dano ao Longo do Tempo (Defaults do Prefab)")]
    [SerializeField]
    [Tooltip("Dano por segundo do DoT. Substituído pelo TowerArcane via Initialize() em runtime.")]
    private float dotDamagePerSecond = 5f;

    [SerializeField]
    [Tooltip("Duração do DoT em segundos. Substituído pelo TowerArcane via Initialize() em runtime.")]
    private float dotDuration = 3f;

    // ─── OVERRIDE DE INICIALIZAÇÃO ───────────────────────────────────────────

    /// <summary>
    /// Inicializa o projétil com os parâmetros da torre.
    /// Se dotDps e dotDuration não forem fornecidos (= 0), usa os valores do Inspector.
    /// Isto permite que o prefab funcione com valores padrão mesmo sem chamada explícita.
    /// </summary>
    public override void Initialize(EnemyBase target, float dmg, float dmgMultiplier = 1f,
                                    float dotDps = 0f, float dotDuration = 0f)
    {
        // Usar valores do Inspector como fallback se a torre não os sobrepuser
        float effectiveDotDps      = dotDps      > 0f ? dotDps      : dotDamagePerSecond;
        float effectiveDotDuration = dotDuration > 0f ? dotDuration : this.dotDuration;

        base.Initialize(target, dmg, dmgMultiplier, effectiveDotDps, effectiveDotDuration);
    }

    // ─── OVERRIDE DE IMPACTO ─────────────────────────────────────────────────

    /// <summary>
    /// Ao atingir o alvo aplica dano imediato e inicia o DoT.
    /// O método ApplyDot da EnemyBase trata a imunidade e stacking de DoTs.
    /// </summary>
    protected override void OnHit()
    {
        if (_target == null)
        {
            ReturnToPool();
            return;
        }

        // 1. Dano imediato (multiplicado por bónus de sinergia, se aplicável)
        _target.TakeDamage(damage * _damageMultiplier);

        // 2. DoT — ApplyDot na EnemyBase respeita imunidade e renova duração se já ativo
        if (_dotDamagePerSec > 0f)
            _target.ApplyDot(_dotDamagePerSec, _dotDuration);

        ReturnToPool();
    }
}
