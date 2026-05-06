// ObjectPool.cs
// Pool genérico reutilizável para inimigos e projéteis.
// Evita Instantiate/Destroy frequentes, melhorando significativamente a performance.
// Autor: Grupo Iron Bastion | ISTEC Lisboa | UC Gamificação 2024/25
// Data: Abril 2025

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Pool genérico de objetos Unity. Instancia objetos antecipadamente e reutiliza-os.
/// Suporta expansão automática se o pool inicial esgotar.
/// </summary>
public class ObjectPool : MonoBehaviour
{
    // ─── SINGLETON ───────────────────────────────────────────────────────────

    public static ObjectPool Instance { get; private set; }

    // ─── ESTRUTURA INTERNA ───────────────────────────────────────────────────

    private readonly Dictionary<string, Queue<GameObject>> _pools = new();
    private readonly Dictionary<string, GameObject> _prefabRegistry = new();

    // ─── UNITY ───────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // ─── API PÚBLICA ─────────────────────────────────────────────────────────

    /// <summary>
    /// Pré-aquece o pool instanciando um número inicial de objetos.
    /// Deve ser chamado no início da cena para cada prefab necessário.
    /// </summary>
    /// <param name="prefab">Prefab a usar como template.</param>
    /// <param name="initialCount">Quantidade de objetos a pré-instanciar.</param>
    public void Prewarm(GameObject prefab, int initialCount)
    {
        string key = prefab.name;
        if (!_pools.ContainsKey(key))
        {
            _pools[key] = new Queue<GameObject>();
            _prefabRegistry[key] = prefab;
        }

        for (int i = 0; i < initialCount; i++)
        {
            GameObject obj = CreateNew(prefab, key);
            obj.SetActive(false);
            _pools[key].Enqueue(obj);
        }
    }

    /// <summary>
    /// Obtém um objeto do pool. Se o pool estiver vazio, cria um novo automaticamente.
    /// </summary>
    /// <param name="prefab">Prefab cujo pool é consultado.</param>
    /// <param name="position">Posição world onde o objeto deve aparecer.</param>
    /// <param name="rotation">Rotação inicial do objeto.</param>
    public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        string key = prefab.name;

        if (!_pools.ContainsKey(key))
            Prewarm(prefab, 1);

        GameObject obj;
        if (_pools[key].Count > 0)
        {
            obj = _pools[key].Dequeue();
        }
        else
        {
            // Pool esgotado — expandir automaticamente
            obj = CreateNew(_prefabRegistry.ContainsKey(key) ? _prefabRegistry[key] : prefab, key);
        }

        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.SetActive(true);
        return obj;
    }

    /// <summary>
    /// Devolve um objeto ao pool. O objeto é desativado e fica disponível para reutilização.
    /// </summary>
    /// <param name="obj">Objeto a devolver.</param>
    /// <param name="prefabKey">Nome do prefab original (obj.name sem "(Clone)").</param>
    public void Return(GameObject obj, string prefabKey)
    {
        obj.SetActive(false);

        if (!_pools.ContainsKey(prefabKey))
            _pools[prefabKey] = new Queue<GameObject>();

        _pools[prefabKey].Enqueue(obj);
    }

    /// <summary>Versão simplificada de Return — extrai o nome da chave a partir do nome do objeto.</summary>
    public void Return(GameObject obj)
    {
        // Remove o sufixo "(Clone)" que o Unity adiciona automaticamente
        string key = obj.name.Replace("(Clone)", "").Trim();
        Return(obj, key);
    }

    // ─── PRIVADO ─────────────────────────────────────────────────────────────

    private GameObject CreateNew(GameObject prefab, string key)
    {
        GameObject obj = Instantiate(prefab, transform);
        obj.name = prefab.name; // Remove o "(Clone)" do nome
        if (!_prefabRegistry.ContainsKey(key))
            _prefabRegistry[key] = prefab;
        return obj;
    }
}
