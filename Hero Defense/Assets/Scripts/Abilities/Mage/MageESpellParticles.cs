using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MageESpellParticles : NetworkBehaviour
{
    [Header("Setup Fields")]
    public GameObject particles;
    private ParticleSystem[] pSystems;
    [Space]
    public Color dmgColor;
    public Color healColor;

    private Color defaultColor;

    public void Init()
    {
        pSystems = particles.GetComponentsInChildren<ParticleSystem>();
    }

    public void SpawnParticles()
    {
        GameObject p = Instantiate(particles, transform);
        pSystems = p.GetComponentsInChildren<ParticleSystem>();
    }

    private void Start()
    {
        //defaultColor = pSystems[0].main.startColor.color;



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
