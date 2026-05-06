using UnityEngine;

public class PlacementGhost : MonoBehaviour
{
    [Header("Range Indicator")]
    [SerializeField] private GameObject rangeIndicator;
    [SerializeField] private Projector rangeProjector;
    [SerializeField] private float rangeIndicatorScaleMultiplier = 2.0f; // Projector size is orthographic size

    public void ShowRangeIndicator(bool show)
    {
        if (rangeIndicator != null)
        {
            rangeIndicator.SetActive(show);
        }
        if (rangeProjector != null)
        {
            rangeProjector.enabled = show;
        }
    }

    public void SetRange(float range)
    {
        if (rangeIndicator != null)
        {
            rangeIndicator.transform.localScale = new Vector3(range * 2, 0.01f, range * 2);
        }
        if (rangeProjector != null)
        {
            rangeProjector.orthographicSize = range * rangeIndicatorScaleMultiplier;
        }
    }

    private void Update()
    {
        // This script is attached to the ghost tower that follows the mouse.
        // We need to adjust the position of the range indicator to follow the terrain.
        if (rangeIndicator != null && rangeIndicator.activeSelf)
        {
            if (Physics.Raycast(transform.position + Vector3.up * 5f, Vector3.down, out RaycastHit hit, 10f))
            {
                rangeIndicator.transform.position = hit.point + new Vector3(0, 0.1f, 0);
                rangeIndicator.transform.up = hit.normal;
            }
        }
    }
}
