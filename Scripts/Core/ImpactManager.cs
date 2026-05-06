using UnityEngine;

public class ImpactManager : MonoBehaviour
{
    #region Singleton
    public static ImpactManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    #endregion

    [Header("Particle Pools")]
    [SerializeField] private string hitParticleTag = "HitParticle";

    public void PlayHitParticle(Vector3 position)
    {
        if (ObjectPooler.Instance != null)
        {
            GameObject particle = ObjectPooler.Instance.SpawnFromPool(hitParticleTag, position, Quaternion.identity);
            if (particle != null)
            {
                ParticleSystem particleSystem = particle.GetComponent<ParticleSystem>();
                if (particleSystem != null)
                {
                    particleSystem.Play();
                }
            }
        }
    }
}
