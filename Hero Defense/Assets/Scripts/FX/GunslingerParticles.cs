using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GunslingerParticles : NetworkBehaviour
{
    public ParticleSystem aaParticles;
    CharacterCombat cb;
    AbilityGunslingerR abilityR;

    // Use this for initialization
    void Start()
    {
        cb = GetComponent<CharacterCombat>();
        abilityR = GetComponent<AbilityGunslingerR>();
        cb.OnAttackEcecuted += PlayParticleEffect;
        abilityR.OnProjectileFired += PlayParticleEffect;
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
