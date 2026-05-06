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

    [Header("Stylized Grid (optional)")]
    [SerializeField]
    [Tooltip("Custom grass tile prefab. If null, fallback cubes are used (built by bootstrap).")]
    private GameObject grassTilePrefab;

    [SerializeField]
    [Tooltip("Grid dimensions used only when rebuilding at runtime via RebuildGrid().")]
    private int gridWidth  = 18;
    [SerializeField]
    private int gridHeight = 18;

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
        GameEvents.OnPlacementCancelled?.Invoke();
    }

    /// <summary>
    /// Rebuilds the grid at runtime using grassTilePrefab (or fallback cubes).
    /// Call this if you want to swap tile art at runtime.
    /// Existing GridCell children of "Grid" are destroyed first.
    /// </summary>
    public void RebuildGrid()
    {
        Transform gridParent = transform.Find("Grid");
        if (gridParent == null)
        {
            GameObject gp = new GameObject("Grid");
            gp.transform.SetParent(transform);
            gridParent = gp.transform;
        }

        // Clear old tiles
        for (int i = gridParent.childCount - 1; i >= 0; i--)
            Destroy(gridParent.GetChild(i).gameObject);

        GameObject tilePrefab = grassTilePrefab;

        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                Vector3 pos = new Vector3(x + 0.5f, 0f, z + 0.5f);

                GameObject cell = tilePrefab != null
                    ? Instantiate(tilePrefab, pos, Quaternion.identity, gridParent)
                    : CreateFallbackCell(pos, gridParent);

                cell.name = $"Cell_{x}_{z}";

                if (cell.GetComponent<GridCell>() == null)
                    cell.AddComponent<GridCell>();

                if (cell.GetComponent<BoxCollider>() == null)
                {
                    BoxCollider bc = cell.AddComponent<BoxCollider>();
                    bc.size = new Vector3(1f, 20f, 1f);
                }

                cell.layer = LayerMask.NameToLayer("Buildable");
            }
        }
    }

    private static GameObject CreateFallbackCell(Vector3 pos, Transform parent)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.transform.SetParent(parent);
        go.transform.position    = pos;
        go.transform.localScale  = new Vector3(0.94f, 0.06f, 0.94f);
        Destroy(go.GetComponent<BoxCollider>());
        BoxCollider bc = go.AddComponent<BoxCollider>();
        bc.size = new Vector3(1f, 20f, 1f);
        return go;
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

            // Keep range preview centred on the current hovered cell
            if (_pendingTowerData != null)
                RangePreview.ShowAt(
                    _hoveredCell.transform.position,
                    _pendingTowerData.GetRange(1));
        }
        else
        {
            RangePreview.Hide();
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
