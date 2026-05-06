// GhostPlacer.cs
// Shows a semi-transparent "ghost" of the tower being placed.
// Follows the mouse across the grid during placement mode.
// Colors green on valid cells, red on blocked/occupied cells.

using UnityEngine;

/// <summary>
/// Singleton that manages the ghost tower preview during placement mode.
/// Instantiates a copy of the tower prefab with transparent materials,
/// positions it under the mouse cursor, and tints it based on placement validity.
/// </summary>
public class GhostPlacer : MonoBehaviour
{
    // ─── SINGLETON ───────────────────────────────────────────────────────────

    public static GhostPlacer Instance { get; private set; }

    // ─── INSPECTOR ───────────────────────────────────────────────────────────

    [Header("Ghost Appearance")]
    [Tooltip("Alpha of the ghost on a valid cell.")]
    [SerializeField] [Range(0.1f, 0.8f)] private float validAlpha   = 0.55f;
    [Tooltip("Alpha of the ghost on an invalid cell.")]
    [SerializeField] [Range(0.1f, 0.8f)] private float invalidAlpha = 0.45f;
    [SerializeField] private Color validTint   = new Color(0.4f, 1f, 0.4f);
    [SerializeField] private Color invalidTint = new Color(1f, 0.3f, 0.3f);

    [Header("Raycasting")]
    [SerializeField] private LayerMask buildableLayerMask;
    [SerializeField] private Camera    mainCamera;

    // ─── STATE ───────────────────────────────────────────────────────────────

    private GameObject _ghostInstance;
    private TowerData  _currentData;
    private bool       _isActive;

    // ─── UNITY ───────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (mainCamera == null) mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        GameEvents.OnTowerPlaced      += OnPlacementFinished;
        GameEvents.OnPlacementCancelled += OnPlacementCancelled;
    }

    private void OnDisable()
    {
        GameEvents.OnTowerPlaced      -= OnPlacementFinished;
        GameEvents.OnPlacementCancelled -= OnPlacementCancelled;
    }

    private void Update()
    {
        if (!_isActive || _ghostInstance == null) return;

        GridCell cell = GetCellUnderMouse();

        if (cell != null)
        {
            // Snap ghost to cell centre (slightly above ground)
            Vector3 pos = cell.transform.position;
            pos.y = 0.5f;
            _ghostInstance.transform.position = pos;
            _ghostInstance.SetActive(true);

            // Update range circle position
            if (_currentData != null)
                RangePreview.ShowAt(pos, _currentData.GetRange(1));

            ApplyGhostTint(cell.IsBuildable);
        }
        else
        {
            _ghostInstance.SetActive(false);
            RangePreview.Hide();
        }
    }

    // ─── PUBLIC API ──────────────────────────────────────────────────────────

    /// <summary>Starts ghost placement mode for the given tower type.</summary>
    public void BeginGhost(TowerData data)
    {
        EndGhost();

        _currentData = data;
        _isActive    = true;

        if (data?.prefab == null) return;

        _ghostInstance = Instantiate(data.prefab, Vector3.zero, Quaternion.identity);
        _ghostInstance.name = "GhostTower";

        DisableScripts(_ghostInstance);
        MakeTransparent(_ghostInstance, validTint, validAlpha);
    }

    /// <summary>Destroys the ghost and exits placement preview mode.</summary>
    public void EndGhost()
    {
        if (_ghostInstance != null)
        {
            Destroy(_ghostInstance);
            _ghostInstance = null;
        }
        _isActive    = false;
        _currentData = null;
        RangePreview.Hide();
    }

    // ─── EVENT HANDLERS ──────────────────────────────────────────────────────

    private void OnPlacementFinished(TowerBase _) => EndGhost();
    private void OnPlacementCancelled()           => EndGhost();

    // ─── HELPERS ─────────────────────────────────────────────────────────────

    private GridCell GetCellUnderMouse()
    {
        if (mainCamera == null) return null;
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 200f, buildableLayerMask))
            return hit.collider.GetComponent<GridCell>();
        return null;
    }

    private void ApplyGhostTint(bool valid)
    {
        Color tint  = valid ? validTint   : invalidTint;
        float alpha = valid ? validAlpha  : invalidAlpha;
        MakeTransparent(_ghostInstance, tint, alpha);
    }

    /// <summary>Replaces every renderer material on the ghost with a transparent tinted version.</summary>
    private static void MakeTransparent(GameObject root, Color tint, float alpha)
    {
        foreach (Renderer r in root.GetComponentsInChildren<Renderer>())
        {
            // Work on instance materials so the originals are never modified
            Material[] mats = r.materials;
            for (int i = 0; i < mats.Length; i++)
            {
                Material m = new Material(mats[i]);
                SetMaterialTransparent(m);
                Color c = tint;
                c.a = alpha;
                m.color = c;
                mats[i] = m;
            }
            r.materials = mats;
        }
    }

    private static void SetMaterialTransparent(Material m)
    {
        m.SetFloat("_Mode", 3);
        m.SetInt("_SrcBlend",  (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        m.SetInt("_DstBlend",  (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        m.SetInt("_ZWrite", 0);
        m.DisableKeyword("_ALPHATEST_ON");
        m.EnableKeyword("_ALPHABLEND_ON");
        m.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        m.renderQueue = 3000;
    }

    /// <summary>Disables all MonoBehaviour scripts on the ghost so it doesn't fire or target anything.</summary>
    private static void DisableScripts(GameObject root)
    {
        foreach (MonoBehaviour mb in root.GetComponentsInChildren<MonoBehaviour>())
            mb.enabled = false;

        // Also disable colliders so the ghost doesn't interfere with raycasts
        foreach (Collider col in root.GetComponentsInChildren<Collider>())
            col.enabled = false;
    }
}
