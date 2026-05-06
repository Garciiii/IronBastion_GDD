// CameraController.cs
// Câmara isométrica 3D com suporte a pan via rato e teclado.
// A câmara é fixa (sem zoom) — estilo Clash of Clans / tower defense clássico.
// Autor: Grupo Iron Bastion | ISTEC Lisboa | UC Gamificação 2024/25
// Data: Abril 2025

using UnityEngine;

/// <summary>
/// Controla o pan da câmara isométrica 3D do Iron Bastion.
/// Pan com botão do meio do rato e teclas WASD / setas.
/// Posição limitada por bordas configuráveis para manter o nível visível.
/// Sem zoom — câmara Orthographic de tamanho fixo.
/// </summary>
public class CameraController : MonoBehaviour
{
    // ─── CONFIGURAÇÃO ────────────────────────────────────────────────────────

    [Header("Velocidade")]
    [SerializeField]
    [Tooltip("Velocidade base de pan (unidades/segundo) para rato e teclado.")]
    private float panSpeed = 20f;

    [SerializeField]
    [Tooltip("Sensibilidade do pan com o botão do meio do rato.")]
    private float mouseSensitivity = 1.5f;

    [SerializeField]
    [Tooltip("Multiplicador de velocidade para pan via teclado WASD / setas.")]
    [Range(0.1f, 2f)]
    private float keyboardMultiplier = 0.8f;

    [Header("Limites de Pan")]
    [SerializeField]
    [Tooltip("Limites de pan no eixo X world (min, max).")]
    private Vector2 panLimitX = new Vector2(-5f, 35f);

    [SerializeField]
    [Tooltip("Limites de pan no eixo Z world (min, max).")]
    private Vector2 panLimitZ = new Vector2(-5f, 25f);

    [Header("Posição Inicial")]
    [SerializeField]
    [Tooltip("Resetar para a posição inicial com a tecla Home.")]
    private bool enableHomeReset = true;

    // ─── ESTADO ──────────────────────────────────────────────────────────────

    private Vector3 _defaultPosition;
    private Quaternion _defaultRotation;

    // ─── UNITY ───────────────────────────────────────────────────────────────

    private void Awake()
    {
        // Guardar posição isométrica inicial para poder resetar
        _defaultPosition = transform.position;
        _defaultRotation = transform.rotation;
    }

    private void Update()
    {
        // Não fazer pan durante pausa
        if (GameManager.Instance != null && GameManager.Instance.IsPaused) return;

        HandleMousePan();
        HandleKeyboardPan();
        HandleHomeReset();
    }

    // ─── PAN COM RATO ────────────────────────────────────────────────────────

    /// <summary>
    /// Pan com o botão do meio do rato (scroll wheel button).
    /// Arrastar na direção oposta ao delta do rato (inverso — comportamento intuitivo de câmara).
    /// </summary>
    private void HandleMousePan()
    {
        if (!Input.GetMouseButton(2)) return;

        float deltaX = Input.GetAxis("Mouse X");
        float deltaZ = Input.GetAxis("Mouse Y");

        // Inverter para que a câmara "siga" o rato
        Vector3 pan = new Vector3(-deltaX, 0f, -deltaZ) * mouseSensitivity;
        ApplyPan(pan);
    }

    // ─── PAN COM TECLADO ─────────────────────────────────────────────────────

    /// <summary>
    /// Pan com WASD ou teclas de seta.
    /// Usa GetAxisRaw para resposta imediata sem interpolação.
    /// </summary>
    private void HandleKeyboardPan()
    {
        float h = Input.GetAxisRaw("Horizontal"); // A/D ou ←/→
        float v = Input.GetAxisRaw("Vertical");   // W/S ou ↑/↓

        if (Mathf.Approximately(h, 0f) && Mathf.Approximately(v, 0f)) return;

        Vector3 pan = new Vector3(h, 0f, v) * keyboardMultiplier;
        ApplyPan(pan);
    }

    // ─── RESET ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Reseta a câmara para a posição isométrica padrão ao pressionar Home.
    /// </summary>
    private void HandleHomeReset()
    {
        if (!enableHomeReset) return;
        if (!Input.GetKeyDown(KeyCode.Home)) return;
        transform.position = _defaultPosition;
        transform.rotation = _defaultRotation;
    }

    // ─── UTILITÁRIOS ─────────────────────────────────────────────────────────

    /// <summary>
    /// Aplica o delta de pan, escalado por velocidade e deltaTime, com clamp nos limites.
    /// </summary>
    private void ApplyPan(Vector3 delta)
    {
        Vector3 newPos = transform.position + delta * panSpeed * Time.deltaTime;

        // Clamp dentro dos limites do nível
        newPos.x = Mathf.Clamp(newPos.x, panLimitX.x, panLimitX.y);
        newPos.z = Mathf.Clamp(newPos.z, panLimitZ.x, panLimitZ.y);

        // Manter a altura Y original (câmara fixa em Y)
        newPos.y = transform.position.y;

        transform.position = newPos;
    }
}
