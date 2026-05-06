// HealthBar.cs
// Barra de vida 3D que flutua sobre os inimigos usando Canvas World Space.
// Autor: Grupo Iron Bastion | ISTEC Lisboa | UC Gamificação 2024/25
// Data: Abril 2025

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Barra de vida em World Space que acompanha o inimigo e orienta-se para a câmara.
/// Attach este script ao GameObject da barra de vida (filho do inimigo).
/// </summary>
public class HealthBar : MonoBehaviour
{
    // ─── CONFIGURAÇÃO ────────────────────────────────────────────────────────

    [SerializeField]
    [Tooltip("Imagem de preenchimento da barra (Image com fill = Filled, Horizontal).")]
    private Image fillImage;

    [SerializeField]
    [Tooltip("Gradiente de cor da barra: verde (100%) -> amarelo (50%) -> vermelho (0%).")]
    private Gradient healthGradient;

    // ─── ESTADO ──────────────────────────────────────────────────────────────

    private EnemyBase _enemy;
    private Camera _mainCamera;
    private bool _damageTaken;  // Controla visibilidade para inimigos sem alwaysShowHealthBar

    // ─── UNITY ───────────────────────────────────────────────────────────────

    private void Awake()
    {
        _mainCamera = Camera.main;
        _enemy = GetComponentInParent<EnemyBase>();
    }

    private void Start()
    {
        // Verificar se deve começar visível
        bool alwaysVisible = _enemy?.Data?.alwaysShowHealthBar ?? false;
        gameObject.SetActive(alwaysVisible);
        _damageTaken = alwaysVisible;
        UpdateFill(1f);
    }

    private void LateUpdate()
    {
        if (!gameObject.activeSelf) return;

        // Manter a barra sempre virada para a câmara
        if (_mainCamera != null)
            transform.rotation = Quaternion.LookRotation(
                transform.position - _mainCamera.transform.position
            );
    }

    private void OnEnable()
    {
        // Resetar ao sair do pool
        _damageTaken = false;
        UpdateFill(1f);

        if (_enemy != null)
            gameObject.SetActive(_enemy.Data?.alwaysShowHealthBar ?? false);
    }

    // ─── API PÚBLICA ─────────────────────────────────────────────────────────

    /// <summary>
    /// Atualiza a barra de vida com o valor atual e máximo.
    /// Chamado pelo EnemyBase sempre que toma dano.
    /// </summary>
    public void UpdateHealth(float current, float max)
    {
        float ratio = max > 0f ? Mathf.Clamp01(current / max) : 0f;

        // Mostrar a barra ao primeiro dano (para inimigos sem alwaysShowHealthBar)
        if (!_damageTaken && ratio < 1f)
        {
            _damageTaken = true;
            gameObject.SetActive(true);
        }

        UpdateFill(ratio);
    }

    // ─── PRIVADO ─────────────────────────────────────────────────────────────

    private void UpdateFill(float ratio)
    {
        if (fillImage == null) return;
        fillImage.fillAmount = ratio;

        // Atualizar cor via gradiente
        if (healthGradient != null)
            fillImage.color = healthGradient.Evaluate(ratio);
    }
}
