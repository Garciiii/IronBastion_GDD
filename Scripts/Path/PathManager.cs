// PathManager.cs
// Gere a lista de waypoints que os inimigos seguem no mapa 3D.
// Os waypoints são definidos como Transforms filhos deste GameObject no Inspector.
// Autor: Grupo Iron Bastion | ISTEC Lisboa | UC Gamificação 2024/25
// Data: Abril 2025

using UnityEngine;

/// <summary>
/// Singleton que fornece o caminho dos inimigos através de uma lista de waypoints 3D.
/// Adiciona os Transforms filhos ordenados como waypoints no Inspector.
/// </summary>
public class PathManager : MonoBehaviour
{
    // ─── SINGLETON ───────────────────────────────────────────────────────────

    public static PathManager Instance { get; private set; }

    // ─── CAMPOS ──────────────────────────────────────────────────────────────

    [SerializeField]
    [Tooltip("Cor das linhas Gizmo no editor.")]
    private Color gizmoColor = Color.cyan;

    /// <summary>Array de posições dos waypoints, preenchido automaticamente a partir dos filhos.</summary>
    private Transform[] _waypoints;

    // ─── UNITY ───────────────────────────────────────────────────────────────

    private void Awake()
    {
        // Configurar singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Recolher todos os Transforms filhos como waypoints, pela ordem hierárquica
        _waypoints = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
            _waypoints[i] = transform.GetChild(i);
    }

    // ─── API PÚBLICA ─────────────────────────────────────────────────────────

    /// <summary>Retorna a posição world 3D do waypoint no índice dado.</summary>
    public Vector3 GetWaypointPosition(int index)
    {
        if (_waypoints == null || index < 0 || index >= _waypoints.Length)
            return Vector3.zero;
        return _waypoints[index].position;
    }

    /// <summary>Retorna o número total de waypoints.</summary>
    public int WaypointCount => _waypoints?.Length ?? 0;

    /// <summary>Verifica se o índice dado é o último waypoint (fim do caminho).</summary>
    public bool IsLastWaypoint(int index) => index >= WaypointCount - 1;

    /// <summary>
    /// Calcula a distância total percorrida a partir de um waypoint e posição intermédios.
    /// Usado pelo sistema de targeting "First" para determinar quem está mais avançado.
    /// </summary>
    public float GetRemainingDistance(int currentWaypointIndex, Vector3 currentPosition)
    {
        if (_waypoints == null || WaypointCount == 0) return 0f;

        // Distância até ao próximo waypoint
        float distance = 0f;
        if (currentWaypointIndex < WaypointCount)
            distance += Vector3.Distance(currentPosition, _waypoints[currentWaypointIndex].position);

        // Distância dos waypoints restantes
        for (int i = currentWaypointIndex; i < WaypointCount - 1; i++)
            distance += Vector3.Distance(_waypoints[i].position, _waypoints[i + 1].position);

        return distance;
    }

    // ─── GIZMOS ──────────────────────────────────────────────────────────────

    private void OnDrawGizmos()
    {
        if (transform.childCount < 2) return;

        Gizmos.color = gizmoColor;

        for (int i = 0; i < transform.childCount - 1; i++)
        {
            Transform a = transform.GetChild(i);
            Transform b = transform.GetChild(i + 1);
            Gizmos.DrawLine(a.position, b.position);
            Gizmos.DrawSphere(a.position, 0.15f);
        }

        // Último waypoint
        Gizmos.DrawSphere(transform.GetChild(transform.childCount - 1).position, 0.25f);
    }
}
