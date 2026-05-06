using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Image fillImage;
    [SerializeField] private Gradient gradient;

    private Transform cameraTransform;

    private void Start()
    {
        cameraTransform = Camera.main.transform;
    }

    private void LateUpdate()
    {
        // Make the health bar face the camera
        transform.LookAt(transform.position + cameraTransform.forward);
    }

    public void SetMaxHealth(float maxHealth)
    {
        healthSlider.maxValue = maxHealth;
        healthSlider.value = maxHealth;
        fillImage.color = gradient.Evaluate(1f);
    }

    public void SetHealth(float health)
    {
        healthSlider.value = health;
        fillImage.color = gradient.Evaluate(healthSlider.normalizedValue);
    }
}
