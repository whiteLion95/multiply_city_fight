using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EZ_Pooling;

/// <summary>
/// A base class for particles managers
/// </summary>
public class ParticlesManagerCore : Singleton<ParticlesManagerCore>
{
    [SerializeField] protected ParticlesData particlesData;

    public ParticleSystem PlayParticle(Particles particleName, Vector3 position, Quaternion rotation)
    {
        ParticleData particleData = particlesData[particleName];
        ParticleSystem playingParticle = EZ_PoolManager.Spawn(particleData.Prefab.transform, position + particleData.Offset, rotation).GetComponentInChildren<ParticleSystem>();

        playingParticle.transform.localScale = particleData.Scale;
        playingParticle.Play();
        StartCoroutine(DespawnParticle(playingParticle));

        return playingParticle;
    }

    public void PlayParticle(Particles particleName, Vector3 position, Quaternion rotation, Color color)
    {
        ParticleSystem playingParticle = PlayParticle(particleName, position, rotation);

        ParticleSystem[] particleSystems = playingParticle.GetComponentsInChildren<ParticleSystem>();

        foreach (var particles in particleSystems)
        {
            ParticleSystem.MainModule settings = particles.main;
            settings.startColor = new ParticleSystem.MinMaxGradient(color);
        }
    }

    protected IEnumerator DespawnParticle(ParticleSystem particle)
    {
        yield return new WaitForSeconds(particle.main.duration + 0.5f);
        
        if (particle != null)
            EZ_PoolManager.Despawn(particle.transform.parent);
    }
}