// EnemyFast.cs
// Corrompido Veloz — inimigo rápido com resistência a slow.
// HP: 50 | Velocidade: 4.0 | Recompensa: 6 ouro | Dano: 1 vida | Res. Slow: 50%
// Autor: Grupo Iron Bastion | ISTEC Lisboa | UC Gamificação 2024/25
// Data: Abril 2025

using UnityEngine;

/// <summary>
/// Corrompido Veloz — rápido e difícil de atingir com torres de área.
/// A resistência a slow (50%) é configurada no EnemyData_Fast.asset.
/// </summary>
public class EnemyFast : EnemyBase
{
    // O Corrompido Veloz não tem comportamento especial adicional além
    // da resistência a slow, que é gerida pela EnemyBase via EnemyData.slowResistance.
    // Esta subclasse existe para prefabs e lógica futura (ex: efeito visual de velocidade).

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
    }

    protected override void Update()
    {
        base.Update();
    }
}
