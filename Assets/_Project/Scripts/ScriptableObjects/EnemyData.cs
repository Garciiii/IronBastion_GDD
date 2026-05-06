// EnemyData.cs
// ScriptableObject com todos os dados configuráveis de um tipo de inimigo.
// Autor: Grupo Iron Bastion | ISTEC Lisboa | UC Gamificação 2024/25
// Data: Abril 2025

using UnityEngine;

/// <summary>
/// Asset de dados de um tipo de inimigo. Cria um asset por tipo em Data/Enemies/.
/// </summary>
[CreateAssetMenu(fileName = "EnemyData_New", menuName = "IronBastion/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Identificação")]
    [Tooltip("Nome visível ao jogador.")]
    public string enemyName = "Inimigo";

    [Tooltip("Descrição de lore para tooltip.")]
    [TextArea(2, 4)]
    public string description = "";

    [Tooltip("Prefab 3D deste inimigo.")]
    public GameObject prefab;

    // ─── ESTATÍSTICAS BASE ───────────────────────────────────────────────────

    [Header("Estatísticas")]
    [Tooltip("Pontos de vida máximos.")]
    public float maxHealth = 100f;

    [Tooltip("Velocidade de movimento em unidades Unity por segundo.")]
    public float moveSpeed = 2f;

    [Tooltip("Recompensa em ouro ao ser eliminado.")]
    public int goldReward = 10;

    [Tooltip("Vidas que o jogador perde quando este inimigo chega à base.")]
    public int livesLost = 1;

    // ─── RESISTÊNCIAS ────────────────────────────────────────────────────────

    [Header("Resistências")]
    [Tooltip("Percentagem de resistência ao slow (0 = sem resistência, 0.5 = recebe metade do slow).")]
    [Range(0f, 1f)]
    public float slowResistance = 0f;

    [Tooltip("Se verdadeiro, este inimigo fica imune a DoT durante alguns segundos após receber dano.")]
    public bool hasDotImmunity = false;

    [Tooltip("Duração da imunidade a DoT após receber dano (apenas se hasDotImmunity = true).")]
    public float dotImmunityDuration = 2f;

    // ─── ESCALAMENTO ─────────────────────────────────────────────────────────

    [Header("Visual 3D")]
    [Tooltip("Escala do modelo 3D em relação ao padrão (1.0 = normal).")]
    public float modelScale = 1f;

    [Tooltip("Cor do material placeholder.")]
    public Color placeholderColor = Color.green;

    [Tooltip("A barra de vida é sempre visível (true) ou só após primeiro dano (false)?")]
    public bool alwaysShowHealthBar = false;
}
