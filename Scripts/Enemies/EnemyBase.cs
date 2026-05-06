// EnemyBase.cs
// Classe base para todos os inimigos do jogo Iron Bastion.
// Gere o movimento 3D por waypoints, vida, slow, DoT e morte.
// Autor: Grupo Iron Bastion | ISTEC Lisboa | UC Gamificação 2024/25
// Data: Abril 2025

using System.Collections;
using UnityEngine;

/// <summary>
/// Classe base que todos os inimigos herdam.
/// Implementa movimento por waypoints 3D, sistema de vida, slow e DoT.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class EnemyBase : MonoBehaviour
{
    // ─── CONFIGURAÇÃO ────────────────────────────────────────────────────────

    [Header("Dados do Inimigo")]
    [SerializeField]
    [Tooltip("ScriptableObject com os dados base deste inimigo.")]
    protected EnemyData data;

    // ─── ESTADO ──────────────────────────────────────────────────────────────

    protected float _currentHealth;
    protected float _currentSpeed;
    protected bool _isDead;
    protected int _waypointIndex;

    // Slow
    private float _slowTimer;
    private float _activeSlowPercent;

    // DoT
    private Coroutine _dotCoroutine;
    private bool _isDotImmune;

    // Referências
    private Rigidbody _rigidbody;
    private HealthBar _healthBar;

    // ─── PROPRIEDADES PÚBLICAS ───────────────────────────────────────────────

    /// <summary>Vidas que o jogador perde quando este inimigo chega à base.</summary>
    public int LivesLost => data != null ? data.livesLost : 1;

    /// <summary>HP atual.</summary>
    public float CurrentHealth => _currentHealth;

    /// <summary>HP máximo.</summary>
    public float MaxHealth => data != null ? data.maxHealth : 100f;

    /// <summary>Distância restante até ao fim do caminho (usada para targeting "First").</summary>
    public float DistanceToEnd { get; private set; }

    // ─── UNITY ───────────────────────────────────────────────────────────────

    /// <summary>Dados do ScriptableObject para acesso público (ex: HealthBar).</summary>
    public EnemyData Data => data;

    protected virtual void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.isKinematic = true;   // Movimento controlado por código, não por física
        _rigidbody.useGravity = false;
        _healthBar = GetComponentInChildren<HealthBar>();
    }

    protected virtual void OnEnable()
    {
        // Reinicializar ao sair do pool
        if (data != null)
        {
            _currentHealth = data.maxHealth;
            _currentSpeed = data.moveSpeed;
        }
        _isDead = false;
        _waypointIndex = 0;
        _slowTimer = 0f;
        _activeSlowPercent = 0f;
        _isDotImmune = false;
        _dotCoroutine = null;
    }

    protected virtual void Update()
    {
        if (_isDead) return;

        // Atualizar slow timer
        if (_slowTimer > 0f)
        {
            _slowTimer -= Time.deltaTime;
            if (_slowTimer <= 0f)
                RemoveSlow();
        }

        MoveTowardsWaypoint();
        UpdateDistanceToEnd();
    }

    // ─── INICIALIZAÇÃO EXTERNA ───────────────────────────────────────────────

    /// <summary>
    /// Inicializa o inimigo com dados e HP opcionalmente sobrepostos (para boss waves).
    /// </summary>
    public virtual void Initialize(EnemyData enemyData, float healthOverride = 0f)
    {
        data = enemyData;
        _currentHealth = healthOverride > 0f ? healthOverride : enemyData.maxHealth;
        _currentSpeed = enemyData.moveSpeed;

        // Ajustar escala 3D
        transform.localScale = Vector3.one * enemyData.modelScale;

        // Aplicar cor placeholder ao MeshRenderer
        MeshRenderer mr = GetComponentInChildren<MeshRenderer>();
        if (mr != null)
        {
            mr.material = new Material(mr.material);
            mr.material.color = enemyData.placeholderColor;
        }
    }

    // ─── MOVIMENTO ───────────────────────────────────────────────────────────

    private void MoveTowardsWaypoint()
    {
        if (PathManager.Instance == null) return;
        if (_waypointIndex >= PathManager.Instance.WaypointCount)
        {
            ReachEnd();
            return;
        }

        Vector3 target = PathManager.Instance.GetWaypointPosition(_waypointIndex);
        Vector3 direction = (target - transform.position).normalized;

        // Mover apenas no plano XZ (Y fixo)
        direction.y = 0f;
        transform.position += direction * _currentSpeed * Time.deltaTime;

        // Rodar em direção ao movimento
        if (direction != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(direction);

        // Verificar se chegou ao waypoint
        float distToWaypoint = Vector3.Distance(
            new Vector3(transform.position.x, 0f, transform.position.z),
            new Vector3(target.x, 0f, target.z)
        );

        if (distToWaypoint < 0.15f)
        {
            _waypointIndex++;
            if (_waypointIndex >= PathManager.Instance.WaypointCount)
                ReachEnd();
        }
    }

    private void UpdateDistanceToEnd()
    {
        if (PathManager.Instance == null) return;
        DistanceToEnd = PathManager.Instance.GetRemainingDistance(_waypointIndex, transform.position);
    }

    // ─── DANO E MORTE ────────────────────────────────────────────────────────

    /// <summary>Aplica dano direto ao inimigo.</summary>
    public virtual void TakeDamage(float amount)
    {
        if (_isDead) return;

        _currentHealth -= amount;

        // Atualizar barra de vida
        _healthBar?.UpdateHealth(_currentHealth, data != null ? data.maxHealth : 100f);

        // Ativar imunidade a DoT após receber dano (apenas Tanque)
        if (data != null && data.hasDotImmunity)
            StartCoroutine(ActivateDotImmunity());

        if (_currentHealth <= 0f)
            Die();
    }

    /// <summary>
    /// Aplica dano ao longo do tempo.
    /// Respeita a imunidade a DoT do Corrompido Tanque.
    /// </summary>
    public virtual void ApplyDot(float damagePerSecond, float duration)
    {
        if (_isDead || _isDotImmune) return;

        if (_dotCoroutine != null)
            StopCoroutine(_dotCoroutine);
        _dotCoroutine = StartCoroutine(DotRoutine(damagePerSecond, duration));
    }

    private IEnumerator DotRoutine(float damagePerSecond, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration && !_isDead)
        {
            yield return new WaitForSeconds(0.5f);
            elapsed += 0.5f;
            TakeDamage(damagePerSecond * 0.5f);
        }
        _dotCoroutine = null;
    }

    private IEnumerator ActivateDotImmunity()
    {
        _isDotImmune = true;
        yield return new WaitForSeconds(data.dotImmunityDuration);
        _isDotImmune = false;
    }

    protected virtual void Die()
    {
        if (_isDead) return;
        _isDead = true;

        // Notificar o sistema de eventos
        GameEvents.OnEnemyKilled?.Invoke(data != null ? data.goldReward : 0);

        // Devolver ao pool
        if (ObjectPool.Instance != null)
            ObjectPool.Instance.Return(gameObject);
        else
            gameObject.SetActive(false);
    }

    private void ReachEnd()
    {
        if (_isDead) return;
        _isDead = true;

        // Notificar o GameManager
        GameEvents.OnEnemyReachedEnd?.Invoke(this);

        // Devolver ao pool
        if (ObjectPool.Instance != null)
            ObjectPool.Instance.Return(gameObject);
        else
            gameObject.SetActive(false);
    }

    // ─── SLOW ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Aplica um efeito de slow ao inimigo.
    /// Respeita a resistência ao slow de cada tipo.
    /// </summary>
    public virtual void ApplySlow(float slowPercent, float duration)
    {
        if (_isDead || data == null) return;

        // Aplicar resistência ao slow
        float effectiveSlow = slowPercent * (1f - data.slowResistance);
        if (effectiveSlow <= 0f) return;

        // Aplicar apenas se for mais forte que o slow atual
        if (effectiveSlow > _activeSlowPercent)
        {
            _activeSlowPercent = effectiveSlow;
            _currentSpeed = data.moveSpeed * (1f - _activeSlowPercent);
        }

        // Renovar duração
        _slowTimer = Mathf.Max(_slowTimer, duration);
    }

    private void RemoveSlow()
    {
        _activeSlowPercent = 0f;
        _currentSpeed = data != null ? data.moveSpeed : 2f;
    }

    // ─── GIZMOS ──────────────────────────────────────────────────────────────

    private void OnDrawGizmosSelected()
    {
        if (data == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
    }
}
