using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using DG.Tweening;

public class TowerCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [Header("Data")]
    [SerializeField] private TowerData towerData;

    [Header("UI Components")]
    [SerializeField] private Image towerIconImage;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private Image cardBackgroundImage;

    [Header("Tweening")]
    [SerializeField] private float scaleFactor = 1.1f;
    [SerializeField] private float scaleDuration = 0.2f;

    private bool isAffordable = true;
    private Vector3 originalScale;
    private GameObject ghostPreview;
    private PlacementGhost placementGhost;

    private void Start()
    {
        originalScale = transform.localScale;
        if (towerData != null)
        {
            SetupCard();
        }
    }

    private void SetupCard()
    {
        if (towerIconImage != null) towerIconImage.sprite = towerData.towerIcon;
        if (costText != null) costText.text = towerData.cost.ToString();
        if (levelText != null) levelText.text = "Lvl " + towerData.level.ToString();
    }

    public void UpdateAffordability(int currentElixir)
    {
        isAffordable = currentElixir >= towerData.cost;
        if (cardBackgroundImage != null)
        {
            cardBackgroundImage.color = isAffordable ? Color.white : new Color(0.5f, 0.5f, 0.5f, 1f);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isAffordable)
        {
            transform.DOScale(originalScale * scaleFactor, scaleDuration).SetEase(Ease.OutBack);
            // Trigger Shine Shader Effect
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.DOScale(originalScale, scaleDuration);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (isAffordable)
        {
            // Create Ghost Preview
            ghostPreview = new GameObject("GhostPreview");
            Image ghostImage = ghostPreview.AddComponent<Image>();
            ghostImage.sprite = towerData.towerIcon;
            ghostImage.raycastTarget = false;
            ghostPreview.transform.SetParent(transform.root, false); // Set parent to canvas root
            ghostPreview.transform.position = eventData.position;

            // Create Placement Ghost
            placementGhost = Instantiate(towerData.towerPrefab).GetComponent<PlacementGhost>();
            if (placementGhost != null)
            {
                placementGhost.ShowRangeIndicator(true);
            }
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (ghostPreview != null)
        {
            ghostPreview.transform.position = eventData.position;

            Ray ray = Camera.main.ScreenPointToRay(eventData.position);
            if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
            {
                if (placementGhost != null)
                {
                    placementGhost.transform.position = hit.point;
                }
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (ghostPreview != null)
        {
            Destroy(ghostPreview);
        }

        if (placementGhost != null)
        {
            // Try to place the tower
            // If successful, destroy the placementGhost
            // If not, destroy the placementGhost
            Destroy(placementGhost.gameObject);
        }
    }
}
