using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MantisParticles : NetworkBehaviour
{

    public ParticleSystem[] aaParticles;
    [Header("Set only if mortar is attached")]
    public ParticleSystem[] mortarParticles;
    MantisCombat cb;

    // Use this for initialization
    void Start()
    {
        cb = GetComponent<MantisCombat>();
        cb.OnLeftShotFired += PlayAutoAttackParticlesLeft;
        cb.OnRightShotFired += PlayAutoAttackParticlesRight;

        cb.OnLeftMortarFired += PlayMortarParticlesLeft;
        cb.OnRightMortarFired += PlayMortarParticlesRight;
    }

    private void PlayAutoAttackParticlesLeft()
    {
        CmdPlayParticleEffectSynchronized(1);
    }

    private void PlayAutoAttackParticlesRight()
    {
        CmdPlayParticleEffectSynchronized(0);
    }

    private void PlayMortarParticlesLeft()
    {
        CmdPlayMortarParticleEffectSynchronized(1);
    }

    private void PlayMortarParticlesRight()
    {
        CmdPlayMortarParticleEffectSynchronized(0);
    }

    #region Synchronize Particle Effects
    //AA
    [Command]
    void CmdPlayParticleEffectSynchronized(int n)
    {
        RpcPlayPartcilesEffetctOnClients(n);
    }

    [ClientRpc]
    void RpcPlayPartcilesEffetctOnClients(int n)
    {
        aaParticles[n].Play();
    }
    //Mortar
    [Command]
    void CmdPlayMortarParticleEffectSynchronized(int n)
    {
        RpcPlayMortarPartcilesEffetctOnClients(n);
    }

    [ClientRpc]
    void RpcPlayMortarPartcilesEffetctOnClients(int n)
    {
        mortarParticles[n].Play();
    }
    #endregion
}
