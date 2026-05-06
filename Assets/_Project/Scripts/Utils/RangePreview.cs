// RangePreview.cs
// Draws a soft translucent circle on the ground to show a tower's attack range.
// Uses a LineRenderer — no external assets required.
// Appears when hovering a card, during ghost placement, or selecting a placed tower.

using UnityEngine;

/// <summary>
/// Singleton that renders a 3D ground circle to preview tower range.
/// The circle is drawn with a LineRenderer and sits just above the terrain.
///
/// Scene setup: add an empty GameObject named "RangePreview",
/// attach this component, and assign the LineRenderer child.
/// The bootstrap creates this automatically if you use the updated bootstrapper.
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class RangePreview : MonoBehaviour
{
    // ─── SINGLETON ───────────────────────────────────────────────────────────

    public static RangePreview Instance { get; private set; }

    // ─── INSPECTOR ───────────────────────────────────────────────────────────

    [Header("Circle Shape")]
    [SerializeField] [Range(16, 128)] private int segments  = 64;
    [SerializeField] private float yOffset = 0.07f;     // height above ground

    [Header("Appearance")]
    [SerializeField] private float lineWidth   = 0.06f;
    [SerializeField] private Color circleColor = new Color(0.35f, 0.85f, 1f, 0.75f);

    [Header("Filled Disc (optional)")]
    [Tooltip("When assigned, a semi-transparent disc is rendered inside the circle.")]
    [SerializeField] private GameObject discObject;   // flat cylinder / plane with transparent mat

    // ─── RUNTIME ─────────────────────────────────────────────────────────────

    private LineRenderer _lr;
    private bool         _showing;

    // ─── UNITY ───────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        _lr = GetComponent<LineRenderer>();
        ConfigureLineRenderer();
        HideInternal();
    }

    private void OnEnable()
    {
        GameEvents.OnTowerSelected   += OnTowerSelected;
        GameEvents.OnTowerDeselected += OnTowerDeselected;
        GameEvents.OnPlacementCancelled += OnPlacementCancelled;
    }

    private void OnDisable()
    {
        GameEvents.OnTowerSelected   -= OnTowerSelected;
        GameEvents.OnTowerDeselected -= OnTowerDeselected;
        GameEvents.OnPlacementCancelled -= OnPlacementCancelled;
    }

    // ─── STATIC API (called by TowerCard, GhostPlacer) ───────────────────────

    /// <summary>Show circle at a world-space position with the given radius.</summary>
    public static void ShowAt(Vector3 worldPos, float radius)
        => Instance?.ShowInternal(worldPos, radius);

    /// <summary>
    /// Show circle centred at origin — used by card hover before a cell is targeted.
    /// Circle is hidden; the radius is stored so GhostPlacer can pick it up.
    /// </summary>
    public static void ShowRadius(float radius)
        => Instance?.StoreRadius(radius);

    /// <summary>Hide the circle.</summary>
    public static void Hide()
        => Instance?.HideInternal();

    /// <summary>Returns the last stored radius (used by GhostPlacer to draw at hover position).</summary>
    public static float LastRadius { get; private set; }

    // ─── INTERNAL ────────────────────────────────────────────────────────────

    private void ShowInternal(Vector3 centre, float radius)
    {
        LastRadius = radius;
        _showing   = true;
        gameObject.SetActive(true);

        // Build circle points in world space
        _lr.positionCount = segments + 1;
        float step = 2f * Mathf.PI / segments;
        for (int i = 0; i <= segments; i++)
        {
            float angle = i * step;
            _lr.SetPosition(i, new Vector3(
                centre.x + Mathf.Cos(angle) * radius,
                centre.y + yOffset,
                centre.z + Mathf.Sin(angle) * radius
            ));
        }

        // Optional filled disc
        if (discObject != null)
        {
            discObject.SetActive(true);
            discObject.transform.position   = new Vector3(centre.x, centre.y + yOffset * 0.5f, centre.z);
            discObject.transform.localScale = new Vector3(radius * 2f, 0.01f, radius * 2f);
        }
    }

    private void StoreRadius(float radius)
    {
        LastRadius = radius;
        // Don't show yet — GhostPlacer will call ShowAt once it has a position
    }

    private void HideInternal()
    {
        _showing = false;
        gameObject.SetActive(false);
        if (discObject != null) discObject.SetActive(false);
    }

    // ─── GAME EVENT HANDLERS ─────────────────────────────────────────────────

    private void OnTowerSelected(TowerBase tower)
    {
        if (tower == null || tower.Data == null) return;
        ShowInternal(tower.transform.position, tower.EffectiveRange);
    }

    private void OnTowerDeselected()  => HideInternal();
    private void OnPlacementCancelled() => HideInternal();

    // ─── LINE RENDERER SETUP ─────────────────────────────────────────────────

    private void ConfigureLineRenderer()
    {
        _lr.loop             = true;
        _lr.useWorldSpace    = true;
        _lr.startWidth       = lineWidth;
        _lr.endWidth         = lineWidth;
        _lr.positionCount    = segments + 1;
        _lr.receiveShadows   = false;
        _lr.shadowCastingMode= UnityEngine.Rendering.ShadowCastingMode.Off;

        // Build a simple gradient so the circle pulses in opacity
        Gradient g = new Gradient();
        g.SetKeys(
            new[] { new GradientColorKey(circleColor, 0f), new GradientColorKey(circleColor, 1f) },
            new[] { new GradientAlphaKey(0.2f, 0f), new GradientAlphaKey(circleColor.a, 0.3f),
                    new GradientAlphaKey(circleColor.a, 0.7f), new GradientAlphaKey(0.2f, 1f) }
        );
        _lr.colorGradient = g;

        // Use a simple unlit material so the circle is always visible
        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = circleColor;
        _lr.material = mat;
    }
}
