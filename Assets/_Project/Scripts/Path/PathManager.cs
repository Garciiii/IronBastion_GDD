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

    [Header("Stylized Path (optional)")]
    [SerializeField]
    [Tooltip("Stone path tile prefab placed between waypoints. Leave null to keep invisible path.")]
    private GameObject pathTilePrefab;

    [SerializeField]
    [Tooltip("World-space spacing between consecutive path tiles (units).")]
    [Range(0.5f, 2f)]
    private float pathTileSpacing = 1f;

    [SerializeField]
    [Tooltip("Parent transform for spawned path tiles. If null, tiles are children of PathManager.")]
    private Transform pathTileParent;

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

        if (pathTilePrefab != null)
            InstantiatePathTiles();
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

    // ─── PATH TILE INSTANTIATION ─────────────────────────────────────────────

    /// <summary>
    /// Spawns stone path tiles between every pair of consecutive waypoints.
    /// Tiles are evenly spaced along each segment and rotated to face the segment direction.
    /// </summary>
    private void InstantiatePathTiles()
    {
        Transform parent = pathTileParent != null ? pathTileParent : transform;

        for (int i = 0; i < _waypoints.Length - 1; i++)
        {
            Vector3 from = _waypoints[i].position;
            Vector3 to   = _waypoints[i + 1].position;

            float   segmentLength = Vector3.Distance(from, to);
            int     tileCount     = Mathf.Max(1, Mathf.RoundToInt(segmentLength / pathTileSpacing));
            Vector3 dir           = (to - from).normalized;

            // Rotation: face the direction of travel, keep Y-up
            Quaternion rot = dir.sqrMagnitude > 0.001f
                ? Quaternion.LookRotation(new Vector3(dir.x, 0f, dir.z))
                : Quaternion.identity;

            for (int j = 0; j < tileCount; j++)
            {
                float   t   = (j + 0.5f) / tileCount;        // centre of each tile slot
                Vector3 pos = Vector3.Lerp(from, to, t);
                pos.y = 0f;                                    // snap to ground level

                Instantiate(pathTilePrefab, pos, rot, parent);
            }
        }
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
