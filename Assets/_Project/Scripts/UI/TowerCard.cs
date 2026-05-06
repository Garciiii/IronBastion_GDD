// TowerCard.cs
// Individual tower card UI component.
// Displays tower icon, level badge, and cost. Animates on hover.
// Triggers ghost placement mode when clicked.

using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Self-contained card for one tower type in the shop panel.
/// Attach to a prefab that has the structure:
///   Card (this component, Image background)
///     ├── Icon (Image — tower artwork)
///     ├── LevelBadge (Image background)
///     │     └── LevelText (TextMeshProUGUI — "Lv 1")
///     ├── CostRow
///     │     ├── CostIcon (Image — gold/elixir icon)
///     │     └── CostText (TextMeshProUGUI — "75")
///     └── NameText (TextMeshProUGUI — "Archer")
/// </summary>
[RequireComponent(typeof(Image))]
public class TowerCard : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler,
    IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
    // ─── INSPECTOR ───────────────────────────────────────────────────────────

    [Header("Card Elements")]
    [SerializeField] private Image          cardBackground;
    [SerializeField] private Image          towerIcon;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private Image          costIcon;

    [Header("Visual States")]
    [SerializeField] private Color colorNormal    = new Color(0.18f, 0.22f, 0.28f, 0.95f);
    [SerializeField] private Color colorHover     = new Color(0.25f, 0.35f, 0.45f, 1f);
    [SerializeField] private Color colorPressed   = new Color(0.12f, 0.16f, 0.22f, 1f);
    [SerializeField] private Color colorDisabled  = new Color(0.15f, 0.15f, 0.17f, 0.6f);

    [Header("Tween")]
    [SerializeField] [Range(1f, 1.3f)] private float hoverScale   = 1.10f;
    [SerializeField] [Range(0.05f, 0.3f)] private float tweenTime = 0.12f;

    // ─── RUNTIME ─────────────────────────────────────────────────────────────

    private TowerData    _data;
    private int          _currentLevel = 1;
    private Vector3      _baseScale;
    private Coroutine    _scaleTween;
    private Coroutine    _colorTween;
    private bool         _isAvailable;

    // ─── INITIALIZATION ──────────────────────────────────────────────────────

    /// <summary>
    /// Called by TowerCardPanel to configure the card for a specific tower type.
    /// </summary>
    public void Initialize(TowerData data, Sprite icon = null, Sprite goldSprite = null)
    {
        _data      = data;
        _baseScale = transform.localScale;

        if (towerIcon  && icon       != null) towerIcon.sprite  = icon;
        if (costIcon   && goldSprite  != null) costIcon.sprite   = goldSprite;
        if (nameText)  nameText.text  = data.towerName;
        if (costText)  costText.text  = data.cost.ToString();
        if (levelText) levelText.text = "Lv 1";

        RefreshAvailability();

        GameEvents.OnGoldChanged  += OnGoldChanged;
        GameEvents.OnWaveStarted  += OnWaveStarted;
        GameEvents.OnWaveCompleted += OnWaveCompleted;
    }

    private void OnDestroy()
    {
        GameEvents.OnGoldChanged   -= OnGoldChanged;
        GameEvents.OnWaveStarted   -= OnWaveStarted;
        GameEvents.OnWaveCompleted -= OnWaveCompleted;
    }

    // ─── AVAILABILITY ────────────────────────────────────────────────────────

    private void RefreshAvailability()
    {
        if (_data == null) return;

        bool canAfford  = GameManager.Instance != null && GameManager.Instance.CanAfford(_data.cost);
        bool waveActive = WaveManager.Instance  != null && WaveManager.Instance.IsWaveActive;

        _isAvailable = canAfford && !waveActive;

        Color target = _isAvailable ? colorNormal : colorDisabled;
        if (cardBackground) cardBackground.color = target;

        float iconAlpha = _isAvailable ? 1f : 0.4f;
        if (towerIcon) towerIcon.color = new Color(1f, 1f, 1f, iconAlpha);
        if (costText)  costText.color  = _isAvailable
            ? new Color(1f, 0.9f, 0.2f)
            : new Color(0.5f, 0.45f, 0.1f);
    }

    // ─── POINTER EVENTS ──────────────────────────────────────────────────────

    public void OnPointerEnter(PointerEventData _)
    {
        if (!_isAvailable) return;

        TweenScale(_baseScale * hoverScale);
        TweenColor(colorHover);

        // Show range preview at current mouse position (GhostPlacer will update it)
        if (_data != null)
            RangePreview.ShowRadius(_data.GetRange(_currentLevel));
    }

    public void OnPointerExit(PointerEventData _)
    {
        TweenScale(_baseScale);
        TweenColor(_isAvailable ? colorNormal : colorDisabled);
        RangePreview.Hide();
    }

    public void OnPointerDown(PointerEventData _)
    {
        if (!_isAvailable) return;
        TweenScale(_baseScale * 0.95f);
        TweenColor(colorPressed);
    }

    public void OnPointerUp(PointerEventData _)
    {
        if (!_isAvailable) return;
        TweenScale(_baseScale * hoverScale);
        TweenColor(colorHover);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!_isAvailable || _data == null) return;

        GridManager.Instance?.BeginPlacement(_data);
        GhostPlacer.Instance?.BeginGhost(_data);
    }

    // ─── GAME EVENT HANDLERS ─────────────────────────────────────────────────

    private void OnGoldChanged(int _)  => RefreshAvailability();
    private void OnWaveStarted(int a, int b) => RefreshAvailability();
    private void OnWaveCompleted(bool _)  => RefreshAvailability();

    // ─── TWEENS ──────────────────────────────────────────────────────────────

    private void TweenScale(Vector3 target)
    {
        if (_scaleTween != null) StopCoroutine(_scaleTween);
        _scaleTween = StartCoroutine(ScaleRoutine(target));
    }

    private void TweenColor(Color target)
    {
        if (_colorTween != null) StopCoroutine(_colorTween);
        _colorTween = StartCoroutine(ColorRoutine(target));
    }

    private IEnumerator ScaleRoutine(Vector3 target)
    {
        Vector3 start   = transform.localScale;
        float   elapsed = 0f;
        while (elapsed < tweenTime)
        {
            elapsed += Time.unscaledDeltaTime;
            transform.localScale = Vector3.Lerp(start, target, elapsed / tweenTime);
            yield return null;
        }
        transform.localScale = target;
    }

    private IEnumerator ColorRoutine(Color target)
    {
        if (cardBackground == null) yield break;
        Color start   = cardBackground.color;
        float elapsed = 0f;
        while (elapsed < tweenTime)
        {
            elapsed += Time.unscaledDeltaTime;
            cardBackground.color = Color.Lerp(start, target, elapsed / tweenTime);
            yield return null;
        }
        cardBackground.color = target;
    }
}
