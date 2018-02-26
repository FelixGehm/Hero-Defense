using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkSyncAnimations : NetworkBehaviour
{
    [SyncVar]
    float speedPercent;

    CharacterAnimator characterAnimator;

    private void Start()
    {
        characterAnimator = GetComponent<CharacterAnimator>();

        if (!isLocalPlayer)
            characterAnimator.isMovedByAgent = false;
    }

    void Update()
    {
        if (isLocalPlayer)
        {
            speedPercent = characterAnimator.GetSpeedPercent();
            CmdSetSpeedPercent(characterAnimator.GetSpeedPercent());
        }
        else
        {
            characterAnimator.SetSpeedPercent(speedPercent);
        }
    }

    [Command]
    void CmdSetSpeedPercent(float speed)
    {
        characterAnimator.SetSpeedPercent(speed);
    }

}
