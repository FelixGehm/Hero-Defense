using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GunslingerParticles : NetworkBehaviour
{
    public ParticleSystem aaParticles;
    CharacterCombat cb;

    // Use this for initialization
    void Start()
    {
        cb = GetComponent<CharacterCombat>();
        cb.OnAttackEcecuted += PlayParticleEffect;
    }

    private void PlayParticleEffect()
    {
        CmdPlayParticleEffectSynchronized();
    }

    [Command]
    void CmdPlayParticleEffectSynchronized()
    {
        RpcPlayPartcilesEffetctOnClients();
    }

    [ClientRpc]
    void RpcPlayPartcilesEffetctOnClients()
    {
        aaParticles.Play();
    }

}
