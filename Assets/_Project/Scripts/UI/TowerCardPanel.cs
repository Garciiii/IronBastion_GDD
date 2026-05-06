// TowerCardPanel.cs
// Replaces TowerShopUI with a card-based tower shop.
// Spawns TowerCard prefabs dynamically from a TowerData array.
// Attach to the panel GameObject that contains the card layout.

using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Card-based tower shop panel.
/// Dynamically instantiates TowerCard prefabs for each configured tower type.
///
/// Required scene setup:
///   Panel_TowerShop (this component, VerticalLayoutGroup or GridLayoutGroup)
///     └── (TowerCard prefabs instantiated here at runtime)
///
/// Replaces TowerShopUI. Do NOT have both on the same object.
/// </summary>
public class TowerCardPanel : MonoBehaviour
{
    // ─── INSPECTOR ───────────────────────────────────────────────────────────

    [Header("Card Prefab")]
    [Tooltip("Prefab with TowerCard component. Create once and reuse.")]
    [SerializeField] private GameObject cardPrefab;

    [Header("Tower Catalogue (order = display order)")]
    [SerializeField] private TowerEntry[] towers;

    [Header("Shared UI Assets")]
    [Tooltip("Gold coin sprite shown in the cost row of each card.")]
    [SerializeField] private Sprite goldCoinSprite;

    [Header("Panel Header (optional)")]
    [SerializeField] private TextMeshProUGUI headerLabel;

    // ─── NESTED TYPE ─────────────────────────────────────────────────────────

    [System.Serializable]
    public struct TowerEntry
    {
        [Tooltip("ScriptableObject with tower stats, cost, and prefab.")]
        public TowerData data;
        [Tooltip("Artwork shown on the card face (tower portrait or icon).")]
        public Sprite    icon;
    }

    // ─── UNITY ───────────────────────────────────────────────────────────────

    private void Start()
    {
        if (headerLabel != null)
            headerLabel.text = "TOWERS";

        BuildCards();
    }

    // ─── CARD BUILDING ───────────────────────────────────────────────────────

    private void BuildCards()
    {
        if (cardPrefab == null)
        {
            Debug.LogWarning("[TowerCardPanel] cardPrefab is not assigned — building fallback buttons.");
            BuildFallbackButtons();
            return;
        }

        foreach (TowerEntry entry in towers)
        {
            if (entry.data == null) continue;

            GameObject cardGO = Instantiate(cardPrefab, transform);
            TowerCard  card   = cardGO.GetComponent<TowerCard>();

            if (card != null)
                card.Initialize(entry.data, entry.icon, goldCoinSprite);
            else
                Debug.LogWarning($"[TowerCardPanel] Prefab '{cardPrefab.name}' has no TowerCard component.");
        }
    }

    /// <summary>
    /// Fallback: builds simple buttons so the game remains playable when the card prefab is not set.
    /// Mirrors the old TowerShopUI behaviour.
    /// </summary>
    private void BuildFallbackButtons()
    {
        foreach (TowerEntry entry in towers)
        {
            if (entry.data == null) continue;

            TowerData captured = entry.data;

            GameObject btnGO = new GameObject($"Btn_{entry.data.towerName}");
            btnGO.transform.SetParent(transform, false);

            var img = btnGO.AddComponent<UnityEngine.UI.Image>();
            img.color = new Color(0.15f, 0.4f, 0.15f);

            var btn = btnGO.AddComponent<UnityEngine.UI.Button>();
            btn.onClick.AddListener(() =>
            {
                if (GameManager.Instance == null || !GameManager.Instance.CanAfford(captured.cost)) return;
                if (WaveManager.Instance != null && WaveManager.Instance.IsWaveActive) return;
                GridManager.Instance?.BeginPlacement(captured);
                GhostPlacer.Instance?.BeginGhost(captured);
            });

            var rt = btnGO.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(160f, 50f);

            GameObject labelGO = new GameObject("Label");
            labelGO.transform.SetParent(btnGO.transform, false);
            var tmp = labelGO.AddComponent<TextMeshProUGUI>();
            tmp.text      = $"{entry.data.towerName}  {entry.data.cost}g";
            tmp.fontSize  = 18f;
            tmp.color     = Color.white;
            tmp.alignment = TMPro.TextAlignmentOptions.Center;
            var lrt = labelGO.GetComponent<RectTransform>();
            lrt.anchorMin = Vector2.zero;
            lrt.anchorMax = Vector2.one;
            lrt.offsetMin = lrt.offsetMax = Vector2.zero;
        }
    }
}
