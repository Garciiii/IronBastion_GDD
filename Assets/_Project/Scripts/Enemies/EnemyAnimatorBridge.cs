// EnemyAnimatorBridge.cs
// Componente leve que liga o Animator do EnemyVisual ao ciclo de vida do inimigo.
// Adicionar ao prefab EnemyVisual via Integration script.
// Autor: Grupo Iron Bastion | ISTEC Lisboa | UC Gamificação 2024/25

using UnityEngine;

/// <summary>
/// Liga automaticamente o Animator do modelo visual ao estado do inimigo.
/// Toca a animação Walk ao ser ativado.
/// </summary>
public class EnemyAnimatorBridge : MonoBehaviour
{
    // Parâmetro/estado a usar no Animator para caminhar
    [SerializeField] private string walkState = "Walk";
    [SerializeField] private string runState  = "Run";

    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
    }

    private void OnEnable()
    {
        if (_animator == null) return;

        // Tenta reproduzir Walk; se não existir, tenta Run
        if (HasState(_animator, walkState))
            _animator.Play(walkState);
        else if (HasState(_animator, runState))
            _animator.Play(runState);
    }

    // Verifica se um estado existe no Animator (camada 0)
    static bool HasState(Animator anim, string stateName)
    {
        return anim.HasState(0, Animator.StringToHash(stateName));
    }
}
