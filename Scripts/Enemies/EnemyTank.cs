// EnemyTank.cs
// Corrompido Tanque — inimigo lento com muito HP e imunidade a DoT após dano.
// HP: 500 | Velocidade: 0.8 | Recompensa: 40 ouro | Dano: 3 vidas
// Autor: Grupo Iron Bastion | ISTEC Lisboa | UC Gamificação 2024/25
// Data: Abril 2025

using UnityEngine;

/// <summary>
/// Corrompido Tanque — absorve enorme quantidade de dano.
/// Imune a DoT durante 2s após receber qualquer dano.
/// A barra de vida é sempre visível.
/// </summary>
public class EnemyTank : EnemyBase
{
    // A imunidade a DoT é gerida pela EnemyBase via EnemyData.hasDotImmunity = true.
    // A barra de vida sempre visível é gerida pelo HealthBar via EnemyData.alwaysShowHealthBar.
    // As 3 vidas de dano à base são geridas via EnemyData.livesLost = 3.

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
    }

    /// <summary>
    /// O Tanque tem um efeito visual de impacto ligeiramente diferente.
    /// Sobrescreve TakeDamage para adicionar shake na câmara (se implementado).
    /// </summary>
    public override void TakeDamage(float amount)
    {
        base.TakeDamage(amount);
        // Possível extensão futura: CameraShake.Instance?.Shake(0.1f, 0.05f);
    }
}
