// EnemyCommon.cs
// Corrompido Comum — inimigo base de Iron Bastion.
// HP: 100 | Velocidade: 2.0 | Recompensa: 10 ouro | Dano: 1 vida
// Autor: Grupo Iron Bastion | ISTEC Lisboa | UC Gamificação 2024/25
// Data: Abril 2025

using UnityEngine;

/// <summary>
/// Corrompido Comum — o inimigo mais básico.
/// Sem resistências especiais. Comportamento padrão da EnemyBase.
/// Configura-se via EnemyData_Common.asset.
/// </summary>
public class EnemyCommon : EnemyBase
{
    // O Corrompido Comum não tem comportamento especial adicional.
    // Todos os seus parâmetros (HP, velocidade, recompensa) são definidos no ScriptableObject.
    // Esta subclasse existe para permitir prefabs distintos e personalização futura.

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
