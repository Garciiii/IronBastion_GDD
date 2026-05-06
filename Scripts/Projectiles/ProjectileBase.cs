// ProjectileBase.cs
// Projétil base 3D com homing — segue o alvo até o atingir ou perder o alvo.
// Autor: Grupo Iron Bastion | ISTEC Lisboa | UC Gamificação 2024/25
// Data: Abril 2025

using UnityEngine;

/// <summary>
/// Projétil 3D que persegue um alvo específico.
/// Devolve-se ao ObjectPool quando atinge o alvo ou perde-o.
/// </summary>
public class ProjectileBase : MonoBehaviour
{
    // ─── CONFIGURAÇÃO ────────────────────────────────────────────────────────

    [SerializeField]
    [Tooltip("Velocidade de voo em unidades/segundo.")]
    protected float speed = 8f;

    [SerializeField]
    [Tooltip("Dano causado ao atingir o alvo.")]
    protected float damage = 20f;

    [SerializeField]
    [Tooltip("Distância mínima do alvo para considerar impacto.")]
    protected float hitRadius = 0.2f;

    // ─── ESTADO ──────────────────────────────────────────────────────────────

    protected EnemyBase _target;
    protected bool _isActive;

    // Bónus de sinergia (ex: +40% dano do Canhão contra inimigos abrandados)
    protected float _damageMultiplier = 1f;

    // DoT para projéteis da Torre Arcana
    protected float _dotDamagePerSec;
    protected float _dotDuration;

    // ─── UNITY ───────────────────────────────────────────────────────────────

    protected virtual void OnEnable()
    {
        _isActive = true;
    }

    protected virtual void OnDisable()
    {
        _isActive = false;
        _target = null;
    }

    protected virtual void Update()
    {
        if (!_isActive) return;

        // Se o alvo morreu ou desapareceu, destruir o projétil
        if (_target == null || !_target.gameObject.activeInHierarchy)
        {
            ReturnToPool();
            return;
        }

        // Mover em direção ao alvo
        Vector3 direction = (_target.transform.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;

        // Rodar para a direção de movimento
        if (direction != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(direction);

        // Verificar impacto
        float dist = Vector3.Distance(transform.position, _target.transform.position);
        if (dist <= hitRadius)
            OnHit();
    }

    // ─── API PÚBLICA ─────────────────────────────────────────────────────────

    /// <summary>
    /// Inicializa o projétil com alvo, dano e parâmetros opcionais de DoT.
    /// </summary>
    public virtual void Initialize(EnemyBase target, float dmg, float dmgMultiplier = 1f,
                                   float dotDps = 0f, float dotDuration = 0f)
    {
        _target           = target;
        damage            = dmg;
        _damageMultiplier = dmgMultiplier;
        _dotDamagePerSec  = dotDps;
        _dotDuration      = dotDuration;
        _isActive         = true;
    }

    // ─── IMPACTO ─────────────────────────────────────────────────────────────

    protected virtual void OnHit()
    {
        if (_target == null) return;

        // Aplicar dano
        _target.TakeDamage(damage * _damageMultiplier);

        // Aplicar DoT se configurado (Torre Arcana)
        if (_dotDamagePerSec > 0f)
            _target.ApplyDot(_dotDamagePerSec, _dotDuration);

        ReturnToPool();
    }

    protected void ReturnToPool()
    {
        _isActive = false;
        if (ObjectPool.Instance != null)
            ObjectPool.Instance.Return(gameObject);
        else
            gameObject.SetActive(false);
    }
}
