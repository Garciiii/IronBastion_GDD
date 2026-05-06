// TowerData.cs
// ScriptableObject com todos os dados configuráveis de uma torre.
// Permite ajustar balanceamento diretamente no Inspector sem alterar código.
// Autor: Grupo Iron Bastion | ISTEC Lisboa | UC Gamificação 2024/25
// Data: Abril 2025

using UnityEngine;

[CreateAssetMenu(fileName = "New TowerData", menuName = "Tower Defense/Tower Data")]
public class TowerData : ScriptableObject
{
    [Header("General")]
    public string towerName;
    public GameObject towerPrefab;
    public Sprite towerIcon;

    [Header("Stats")]
    public int cost;
    public float range;
    public float fireRate;
    public int damage;
    public int level = 1;
}
