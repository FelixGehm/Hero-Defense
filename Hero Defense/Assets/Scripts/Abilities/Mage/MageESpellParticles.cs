using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MageESpellParticles : NetworkBehaviour
{
    [Header("Setup Fields")]
    public GameObject particles;
    private ParticleSystem[] pSystems;
    public GameObject impactParticles;
    private ParticleSystem[] pSystemsImpact;
    [Space]
    public Color dmgColor;
    public Color healColor;

    private Color defaultColor;

    public void Init()
    {
        pSystems = particles.GetComponentsInChildren<ParticleSystem>();
    }

    private void Start()
    {
        pSystemsImpact = impactParticles.GetComponentsInChildren<ParticleSystem>();
    }

    public void SpawnParticles()
    {
        GameObject p = Instantiate(particles, transform);
        pSystems = p.GetComponentsInChildren<ParticleSystem>();
    }

    public void SpawnImpactParticles(Color c)
    {
        for (int i = 0; i < pSystemsImpact.Length; i++)
        {
            var main = pSystemsImpact[i].main;
            main.startColor = c;
        }

        GameObject p = Instantiate(impactParticles, pSystems[0].gameObject.transform.position, transform.rotation);
        Destroy(p, 1.5f);
    }

    public void SetStartColor(GameObject target)
    {
        Color c = defaultColor;
        if (target.CompareTag("Player"))
        {
            c = healColor;
        }
        else if (target.CompareTag("Enemy"))
        {
            c = dmgColor;
        }

        for (int i = 0; i < pSystems.Length; i++)
        {
            var main = pSystems[i].main;
            main.startColor = c;
        }
    }

    public void SetColor(GameObject target)
    {
        Color c = defaultColor;
        if (target.CompareTag("Player"))
        {
            c = healColor;
        }
        else if (target.CompareTag("Enemy"))
        {
            c = dmgColor;
        }

        for (int i = 0; i < pSystems.Length; i++)
        {
            var main = pSystems[i].main;
            main.startColor = c;
        }
    }
}
