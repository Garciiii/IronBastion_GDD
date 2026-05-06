// TowerIce.cs
// Torre de Gelo — aplica slow contínuo por OverlapSphere 3D. Sem projéteis.
// Custo: 125 | Upgrade: 63 | Slow: 50%->70% | Alcance: 2.5->3.0
// Autor: Grupo Iron Bastion | ISTEC Lisboa | UC Gamificação 2024/25
// Data: Abril 2025

using System.Collections;
using UnityEngine;

/// <summary>
/// Torre de Gelo — torre de suporte que abrandam os inimigos.
/// Não dispara projéteis. Aplica slow por pulsos usando OverlapSphere 3D.
/// Sinergia com Arcana adjacente: slow aumenta de 50% para 65%.
/// Sinergia com Canhão adjacente: Canhão causa +40% dano a abrandados.
/// </summary>
public class TowerIce : TowerBase
{
    // ─── ESTADO ──────────────────────────────────────────────────────────────

    private Coroutine _slowPulseCoroutine;
    private float _activeSlowPercent;  // Pode ser modificado pela sinergia com Arcana

    // ─── INICIALIZAÇÃO ───────────────────────────────────────────────────────

    protected override void OnInitialized()
    {
        _activeSlowPercent = towerData != null ? towerData.GetSlowPercent(_level) : 0.5f;
        CheckSynergies();
        _slowPulseCoroutine = StartCoroutine(SlowPulseRoutine());
    }

    protected override void OnUpgraded()
    {
        _activeSlowPercent = towerData != null ? towerData.GetSlowPercent(_level) : 0.7f;
        CheckSynergies();
    }

    // ─── UPDATE ───────────────────────────────────────────────────────────────

    protected override void Update()
    {
        // A Torre de Gelo não usa o sistema de targeting e disparo da TowerBase.
        // O slow é aplicado pela coroutine SlowPulseRoutine.
        // Apenas verificamos pause/game over.
        if (GameManager.Instance == null || GameManager.Instance.IsGameOver) return;
    }

    private void OnDisable()
    {
        if (_slowPulseCoroutine != null)
        {
            StopCoroutine(_slowPulseCoroutine);
            _slowPulseCoroutine = null;
        }
    }

    // ─── SLOW POR PULSO ──────────────────────────────────────────────────────

    /// <summary>
    /// Aplica slow a todos os inimigos no alcance a cada pulso (0.5s).
    /// Usa OverlapSphere 3D em vez de OverlapCircle 2D.
    /// </summary>
    private IEnumerator SlowPulseRoutine()
    {
        float pulseInterval = towerData != null ? towerData.slowPulseInterval : 0.5f;
        float slowDuration  = towerData != null ? towerData.slowDuration : 0.7f;

        while (true)
        {
            yield return new WaitForSeconds(pulseInterval);

            if (GameManager.Instance == null || GameManager.Instance.IsGameOver) continue;
            if (WaveManager.Instance == null || !WaveManager.Instance.IsWaveActive) continue;

            // Detetar inimigos no alcance efetivo
            Collider[] cols = Physics.OverlapSphere(
                transform.position,
                EffectiveRange,
                LayerMask.GetMask("Enemy")
            );

            foreach (Collider col in cols)
            {
                EnemyBase enemy = col.GetComponentInParent<EnemyBase>();
                if (enemy != null && enemy.gameObject.activeInHierarchy)
                    enemy.ApplySlow(_activeSlowPercent, slowDuration);
            }
        }
    }

    // ─── SINERGIAS ───────────────────────────────────────────────────────────

    /// <summary>
    /// Arcana adjacente aumenta o slow de 50% para 65%.
    /// Notifica Torres Canhão vizinhas da sinergia com Gelo.
    /// </summary>
    private void CheckSynergies()
    {
        if (towerData == null) return;

        float baseSlowPercent = towerData.GetSlowPercent(_level);
        _activeSlowPercent = baseSlowPercent;

        Collider[] cols = Physics.OverlapSphere(transform.position, 1.5f, LayerMask.GetMask("Tower"));

        foreach (Collider col in cols)
        {
            if (col.gameObject == gameObject) continue;

            TowerBase neighbor = col.GetComponentInParent<TowerBase>();
            if (neighbor == null) continue;

            // Arcana + Gelo: slow passa de 50% para 65%
            if (neighbor is TowerArcane)
                _activeSlowPercent = Mathf.Max(_activeSlowPercent, 0.65f);

            // Gelo + Canhão: notificar o Canhão para aplicar bónus
            if (neighbor is TowerCannon cannon)
                cannon.ApplyDamageBonus(1.4f);
        }
    }

    /// <summary>
    /// Sobrescreve o método da TowerBase — Torre de Gelo não usa FindTarget.
    /// </summary>
    protected override void FindTarget() { }

    /// <summary>
    /// Sobrescreve — Torre de Gelo não dispara projéteis.
    /// </summary>
    protected override void Fire() { }
}
