// WaveData.cs
// ScriptableObject que define a composição de uma vaga de inimigos.
// Autor: Grupo Iron Bastion | ISTEC Lisboa | UC Gamificação 2024/25
// Data: Abril 2025

using System;
using UnityEngine;

/// <summary>
/// Representa um grupo de inimigos do mesmo tipo dentro de uma vaga.
/// </summary>
[Serializable]
public struct EnemyGroup
{
    [Tooltip("Dados do tipo de inimigo a spawnar.")]
    public EnemyData enemyData;

    [Tooltip("Quantidade de inimigos deste tipo nesta vaga.")]
    public int count;

    [Tooltip("HP override: 0 = usar o valor padrão do EnemyData. Usado para inimigos boss.")]
    public float healthOverride;
}

/// <summary>
/// Asset de dados de uma vaga. Cria um asset por vaga em Data/Waves/Level_X/.
/// </summary>
[CreateAssetMenu(fileName = "WaveData_New", menuName = "IronBastion/Wave Data")]
public class WaveData : ScriptableObject
{
    [Header("Identificação")]
    [Tooltip("Número desta vaga (1-based).")]
    public int waveNumber = 1;

    [Tooltip("Esta é uma vaga boss?")]
    public bool isBossWave = false;

    // ─── COMPOSIÇÃO DA VAGA ──────────────────────────────────────────────────

    [Header("Composição")]
    [Tooltip("Lista de grupos de inimigos. Serão spawned na ordem da lista.")]
    public EnemyGroup[] enemyGroups;

    [Tooltip("Intervalo em segundos entre o spawn de cada inimigo individual.")]
    public float spawnInterval = 0.8f;

    // ─── ECONOMIA ────────────────────────────────────────────────────────────

    [Header("Economia")]
    [Tooltip("Ouro base garantido no fim da vaga (independente de inimigos eliminados).")]
    public int baseGoldReward = 0;

    // ─── MÉTODOS UTILITÁRIOS ─────────────────────────────────────────────────

    /// <summary>Calcula o número total de inimigos nesta vaga.</summary>
    public int GetTotalEnemyCount()
    {
        int total = 0;
        if (enemyGroups == null) return 0;
        foreach (var group in enemyGroups)
            total += group.count;
        return total;
    }

    /// <summary>Calcula o ouro máximo possível da vaga (soma das recompensas de todos os inimigos + ouro base).</summary>
    public int GetMaxGoldFromKills()
    {
        int total = baseGoldReward;
        if (enemyGroups == null) return total;
        foreach (var group in enemyGroups)
            if (group.enemyData != null)
                total += group.enemyData.goldReward * group.count;
        return total;
    }
}
