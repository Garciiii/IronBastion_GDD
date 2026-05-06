// GridManager.cs
// Gere a grelha 3D do mapa: colocação de torres, highlight e raycasting.
// Autor: Grupo Iron Bastion | ISTEC Lisboa | UC Gamificação 2024/25
// Data: Abril 2025

using UnityEngine;

/// <summary>
/// Singleton que gere todas as células da grelha 3D.
/// Processa cliques do jogador via Raycast para colocar torres.
/// </summary>
public class GridManager : MonoBehaviour
{
    // ─── SINGLETON ───────────────────────────────────────────────────────────

    public static GridManager Instance { get; private set; }

    // ─── CONFIGURAÇÃO ────────────────────────────────────────────────────────

    [SerializeField]
    [Tooltip("Câmara principal usada para o Raycast de seleção.")]
    private Camera mainCamera;

    [SerializeField]
    [Tooltip("Layer mask das células da grelha.")]
    private LayerMask buildableLayerMask;

    [Header("Grid Generation")]
    [SerializeField]
    [Tooltip("Prefab for the rounded tile.")]
    private GameObject roundedTilePrefab;

    [SerializeField]
    [Tooltip("Prefab for the grass tile.")]
    private GameObject grassTilePrefab;

    [SerializeField]
    [Tooltip("Width of the grid.")]
    private int gridWidth = 10;

    [SerializeField]
    [Tooltip("Height of the grid.")]
    private int gridHeight = 10;

    [SerializeField]
    [Tooltip("Size of each cell.")]
    private float cellSize = 1f;

    // ─── ESTADO ──────────────────────────────────────────────────────────────

    private GridCell _hoveredCell;
    private TowerData _pendingTowerData;   // Torre selecionada no shop, ainda por colocar
    private bool _isPlacingTower;

    // ─── UNITY ───────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (mainCamera == null)
            mainCamera = Camera.main;

        GenerateGrid();
    }

    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.IsGameOver) return;

        if (_isPlacingTower)
        {
            HandlePlacementHover();
            HandlePlacementClick();
        }
        else
        {
            HandleTowerSelection();
        }

        // Cancelar colocação com clique direito ou Escape
        if (_isPlacingTower && (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape)))
            CancelPlacement();
    }

    // ─── API PÚBLICA ─────────────────────────────────────────────────────────

    /// <summary>
    /// Inicia o modo de colocação de uma torre.
    /// Chamado pelo TowerShopUI quando o jogador clica em "Comprar".
    /// </summary>
    public void BeginPlacement(TowerData data)
    {
        _pendingTowerData = data;
        _isPlacingTower = true;
        GameEvents.OnTowerDeselected?.Invoke();
    }

    /// <summary>Cancela o modo de colocação sem colocar nenhuma torre.</summary>
    public void CancelPlacement()
    {
        _pendingTowerData = null;
        _isPlacingTower = false;
        ClearAllHighlights();
    }

    private void GenerateGrid()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                Vector3 position = new Vector3(x * cellSize, 0, z * cellSize);
                bool isPath = (x == gridWidth / 2); // Example path logic
                GameObject tilePrefab = isPath ? roundedTilePrefab : grassTilePrefab;
                GameObject tile = Instantiate(tilePrefab, position, Quaternion.identity, transform);
                tile.name = $"Tile_{x}_{z}";

                if (!isPath)
                {
                    // Randomize rotation for grass tiles
                    int randomRotation = Random.Range(0, 4) * 90;
                    tile.transform.rotation = Quaternion.Euler(0, randomRotation, 0);
                }

                GridCell cell = tile.GetComponent<GridCell>();
                if (cell != null)
                {
                    cell.SetState(isPath ? CellState.Blocked : CellState.Buildable);
                }
            }
        }
    }
    // ─── RAYCASTING E HOVER ──────────────────────────────────────────────────

    private void HandlePlacementHover()
    {
        GridCell cell = GetCellUnderMouse();

        if (cell == _hoveredCell) return;

        // Limpar highlight anterior
        if (_hoveredCell != null)
            _hoveredCell.ClearHighlight();

        _hoveredCell = cell;

        if (_hoveredCell != null)
        {
            if (_hoveredCell.IsBuildable)
                _hoveredCell.ShowValidHighlight();
            else
                _hoveredCell.ShowInvalidHighlight();
        }
    }

    private void HandlePlacementClick()
    {
        if (!Input.GetMouseButtonDown(0)) return;
        if (_hoveredCell == null || !_hoveredCell.IsBuildable) return;
        if (_pendingTowerData == null) return;

        // Verificar se o jogador tem ouro suficiente
        if (!GameManager.Instance.CanAfford(_pendingTowerData.cost))
        {
            Debug.Log("[GridManager] Ouro insuficiente para colocar a torre.");
            return;
        }

        PlaceTowerOnCell(_hoveredCell);
    }

    private void HandleTowerSelection()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit)) return;

        TowerBase tower = hit.collider.GetComponentInParent<TowerBase>();
        if (tower != null)
        {
            GameEvents.OnTowerSelected?.Invoke(tower);
        }
        else
        {
            // Clique em zona sem torre — desselecionar
            GameEvents.OnTowerDeselected?.Invoke();
        }
    }

    // ─── COLOCAÇÃO ───────────────────────────────────────────────────────────

    private void PlaceTowerOnCell(GridCell cell)
    {
        // Gastar ouro
        GameManager.Instance.SpendGold(_pendingTowerData.cost);

        // Instanciar a torre na posição da célula
        Vector3 spawnPos = cell.transform.position + Vector3.up * 0.5f;
        GameObject towerObj = Instantiate(_pendingTowerData.prefab, spawnPos, Quaternion.identity);
        TowerBase tower = towerObj.GetComponent<TowerBase>();

        if (tower == null)
        {
            Debug.LogError($"[GridManager] O prefab '{_pendingTowerData.towerName}' não tem componente TowerBase!");
            Destroy(towerObj);
            return;
        }

        // Inicializar a torre com os seus dados
        tower.Initialize(_pendingTowerData, cell);
        cell.PlaceTower(tower);

        // Notificar outros sistemas
        GameEvents.OnTowerPlaced?.Invoke(tower);

        // Terminar o modo de colocação
        _isPlacingTower = false;
        _pendingTowerData = null;
        ClearAllHighlights();
    }

    // ─── UTILITÁRIOS ─────────────────────────────────────────────────────────

    private GridCell GetCellUnderMouse()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, buildableLayerMask))
            return hit.collider.GetComponent<GridCell>();
        return null;
    }

    private void ClearAllHighlights()
    {
        if (_hoveredCell != null)
        {
            _hoveredCell.ClearHighlight();
            _hoveredCell = null;
        }
    }
}
