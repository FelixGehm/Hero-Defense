using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Networking;

public class BossAnimator : EnemyAnimator
{
    public void StartSlamAnimation()
    {
        netAnimator.SetTrigger("slam");
        if (NetworkServer.active) animator.ResetTrigger("slam");
    }

    public void StartCanonIdleAnimation()
    {
        netAnimator.SetTrigger("canons");
        if (NetworkServer.active) animator.ResetTrigger("canons");
    }

    public void StartMortarIdleAnimation()
    {
        netAnimator.SetTrigger("mortar");
        if (NetworkServer.active) animator.ResetTrigger("mortar");
    }

    public void StartSprintAnimation()
    {
        netAnimator.SetTrigger("sprint");
        if (NetworkServer.active) animator.ResetTrigger("sprint");
    }

    public void StartCanonAnimation(int canonIndex)
    {
        netAnimator.SetTrigger("canon" + canonIndex);
        if (NetworkServer.active) animator.ResetTrigger("canon" + canonIndex);
    }
}
