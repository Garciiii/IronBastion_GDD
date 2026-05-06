// GridCell.cs
// Representa uma célula individual da grelha do mapa 3D.
// Cada célula sabe se pode receber uma torre e qual a torre que tem.
// Autor: Grupo Iron Bastion | ISTEC Lisboa | UC Gamificação 2024/25
// Data: Abril 2025

using UnityEngine;

/// <summary>
/// Estados possíveis de uma célula da grelha.
/// </summary>
public enum CellState
{
    /// <summary>Célula livre onde o jogador pode construir.</summary>
    Buildable,
    /// <summary>Célula ocupada por uma torre.</summary>
    Occupied,
    /// <summary>Célula bloqueada (caminho dos inimigos ou borda).</summary>
    Blocked
}

/// <summary>
/// Componente de uma célula da grelha 3D. Gere o estado e o highlight visual.
/// </summary>
[RequireComponent(typeof(MeshRenderer))]
public class GridCell : MonoBehaviour
{
    // ─── CONFIGURAÇÃO ────────────────────────────────────────────────────────

    [SerializeField]
    [Tooltip("Estado inicial desta célula.")]
    private CellState initialState = CellState.Buildable;

    [SerializeField]
    [Tooltip("Material padrão da célula construível.")]
    private Material materialBuildable;

    [SerializeField]
    [Tooltip("Material de highlight válido (verde translúcido).")]
    private Material materialHighlightValid;

    [SerializeField]
    [Tooltip("Material de highlight inválido (vermelho translúcido).")]
    private Material materialHighlightInvalid;

    [SerializeField]
    [Tooltip("Material do caminho dos inimigos.")]
    private Material materialPath;

    // ─── ESTADO ──────────────────────────────────────────────────────────────

    private CellState _state;
    private TowerBase _placedTower;
    private MeshRenderer _renderer;

    // ─── PROPRIEDADES PÚBLICAS ───────────────────────────────────────────────

    /// <summary>Estado atual desta célula.</summary>
    public CellState State => _state;

    /// <summary>A célula está livre para construção?</summary>
    public bool IsBuildable => _state == CellState.Buildable;

    /// <summary>Torre colocada nesta célula (null se livre).</summary>
    public TowerBase PlacedTower => _placedTower;

    // ─── UNITY ───────────────────────────────────────────────────────────────

    private void Awake()
    {
        _renderer = GetComponent<MeshRenderer>();
        SetState(initialState);
    }

    // ─── API PÚBLICA ─────────────────────────────────────────────────────────

    /// <summary>Define o estado desta célula e atualiza o material visual.</summary>
    public void SetState(CellState newState)
    {
        _state = newState;
        UpdateMaterial();
    }

    /// <summary>Coloca uma torre nesta célula.</summary>
    public void PlaceTower(TowerBase tower)
    {
        _placedTower = tower;
        SetState(CellState.Occupied);
    }

    /// <summary>Remove a torre desta célula, libertando-a para construção.</summary>
    public void RemoveTower()
    {
        _placedTower = null;
        SetState(CellState.Buildable);
    }

    /// <summary>Ativa o highlight de colocação válida (verde).</summary>
    public void ShowValidHighlight()
    {
        if (_state != CellState.Buildable) return;
        if (materialHighlightValid != null)
            _renderer.material = materialHighlightValid;
    }

    /// <summary>Ativa o highlight de colocação inválida (vermelho).</summary>
    public void ShowInvalidHighlight()
    {
        if (materialHighlightInvalid != null)
            _renderer.material = materialHighlightInvalid;
    }

    /// <summary>Remove o highlight e restaura o material padrão.</summary>
    public void ClearHighlight()
    {
        UpdateMaterial();
    }

    // ─── PRIVADO ─────────────────────────────────────────────────────────────

    private void UpdateMaterial()
    {
        if (_renderer == null) return;

        switch (_state)
        {
            case CellState.Buildable:
                if (materialBuildable != null)
                    _renderer.material = materialBuildable;
                break;
            case CellState.Blocked:
                if (materialPath != null)
                    _renderer.material = materialPath;
                break;
            case CellState.Occupied:
                // A torre cobre a célula — mantém o material atual
                break;
        }
    }
}
