using UnityEngine;
using TMPro;
using DG.Tweening;

public class DamagePopup : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private TextMeshProUGUI damageText;

    [Header("Animation Settings")]
    [SerializeField] private float moveYAmount = 1.5f;
    [SerializeField] private float duration = 1f;
    [SerializeField] private float scaleUpFactor = 1.5f;

    public void Setup(int damageAmount, bool isCritical)
    {
        damageText.text = damageAmount.ToString();

        if (isCritical)
        {
            damageText.color = Color.yellow;
            transform.localScale *= scaleUpFactor;
        }
        else
        {
            damageText.color = Color.white;
        }

        // Animation
        Sequence sequence = DOTween.Sequence();
        sequence.Append(transform.DOMoveY(transform.position.y + moveYAmount, duration));
        sequence.Join(damageText.DOFade(0, duration).SetEase(Ease.InExpo));
        sequence.OnComplete(() => Destroy(gameObject));
    }

    private void LateUpdate()
    {
        // Make the popup face the camera
        transform.LookAt(transform.position + Camera.main.transform.forward);
    }
}
