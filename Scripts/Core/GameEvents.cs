// GameEvents.cs
// Sistema central de eventos do jogo Iron Bastion.
// Permite comunicação desacoplada entre todos os sistemas do jogo.
// Autor: Grupo Iron Bastion | ISTEC Lisboa | UC Gamificação 2024/25
// Data: Abril 2025

using System;
using UnityEngine;

/// <summary>
/// Sistema de eventos estáticos globais.
/// Todos os sistemas comunicam através destes eventos em vez de referências diretas.
/// Seguindo o padrão Observer para máxima desacoplagem.
/// </summary>
public static class GameEvents
{
    // ─── EVENTOS DE OURO ────────────────────────────────────────────────────

    /// <summary>Disparado quando o ouro do jogador muda. Parâmetro: novo valor total de ouro.</summary>
    public static Action<int> OnGoldChanged;

    /// <summary>Disparado quando o jogador ganha ouro (inimigo eliminado ou bónus).</summary>
    public static Action<int> OnGoldEarned;

    // ─── EVENTOS DE VIDAS ───────────────────────────────────────────────────

    /// <summary>Disparado quando as vidas do jogador mudam. Parâmetro: novo valor de vidas.</summary>
    public static Action<int> OnLivesChanged;

    /// <summary>Disparado quando um inimigo chega à base. Parâmetro: vidas perdidas.</summary>
    public static Action<int> OnBaseDamaged;

    // ─── EVENTOS DE VAGAS ───────────────────────────────────────────────────

    /// <summary>Disparado quando uma nova vaga começa. Parâmetros: número da vaga atual, total de vagas.</summary>
    public static Action<int, int> OnWaveStarted;

    /// <summary>Disparado quando uma vaga termina. Parâmetro: a vaga foi perfeita (sem danos)?</summary>
    public static Action<bool> OnWaveCompleted;

    /// <summary>Disparado durante o countdown entre vagas. Parâmetro: segundos restantes.</summary>
    public static Action<float> OnWaveCountdown;

    /// <summary>Disparado quando todas as vagas do nível foram completadas.</summary>
    public static Action OnAllWavesCompleted;

    // ─── EVENTOS DE INIMIGOS ────────────────────────────────────────────────

    /// <summary>Disparado quando um inimigo é eliminado. Parâmetro: recompensa em ouro.</summary>
    public static Action<int> OnEnemyKilled;

    /// <summary>Disparado quando um inimigo chega ao fim do caminho.</summary>
    public static Action<EnemyBase> OnEnemyReachedEnd;

    // ─── EVENTOS DE TORRES ──────────────────────────────────────────────────

    /// <summary>Disparado quando uma torre é colocada no mapa.</summary>
    public static Action<TowerBase> OnTowerPlaced;

    /// <summary>Disparado quando uma torre é vendida.</summary>
    public static Action<TowerBase> OnTowerSold;

    /// <summary>Disparado quando uma torre é melhorada.</summary>
    public static Action<TowerBase> OnTowerUpgraded;

    /// <summary>Disparado quando o jogador seleciona uma torre existente no mapa.</summary>
    public static Action<TowerBase> OnTowerSelected;

    /// <summary>Disparado quando o jogador desseleciona uma torre.</summary>
    public static Action OnTowerDeselected;

    // ─── EVENTOS DE ESTADO DO JOGO ──────────────────────────────────────────

    /// <summary>Disparado quando o jogo passa para estado de vitória.</summary>
    public static Action<int, int, int> OnVictory; // pontuação, estrelas, vagas perfeitas

    /// <summary>Disparado quando o jogo passa para estado de derrota (vidas = 0).</summary>
    public static Action<int> OnGameOver; // vaga em que ocorreu

    /// <summary>Disparado quando o jogo é pausado.</summary>
    public static Action OnGamePaused;

    /// <summary>Disparado quando o jogo é retomado após pausa.</summary>
    public static Action OnGameResumed;

    // ─── MÉTODO UTILITÁRIO ───────────────────────────────────────────────────

    /// <summary>
    /// Limpa todos os subscritores de todos os eventos.
    /// Deve ser chamado ao carregar uma nova cena para evitar referências nulas.
    /// </summary>
    public static void ClearAllEvents()
    {
        OnGoldChanged     = null;
        OnGoldEarned      = null;
        OnLivesChanged    = null;
        OnBaseDamaged     = null;
        OnWaveStarted     = null;
        OnWaveCompleted   = null;
        OnWaveCountdown   = null;
        OnAllWavesCompleted = null;
        OnEnemyKilled     = null;
        OnEnemyReachedEnd = null;
        OnTowerPlaced     = null;
        OnTowerSold       = null;
        OnTowerUpgraded   = null;
        OnTowerSelected   = null;
        OnTowerDeselected = null;
        OnVictory         = null;
        OnGameOver        = null;
        OnGamePaused      = null;
        OnGameResumed     = null;
    }
}
