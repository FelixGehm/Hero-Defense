using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkSetupEnemy : NetworkSetup
{
    public bool isAnimating = true;

    void Start()
    {
        base.DisableComponents();
        if (isServer)
        {
            // Register Player in PlayerManager
            PlayerManager.instance.RegisterEnemy(gameObject);
        }



        if (isServer)
        {
            GetComponent<EnemyController>().enabled = true;

            // Register Enemy in PlayerManager ?? 
            //PlayerManager.instance.RegisterPlayer(transform.gameObject);
        }
        else
        {
            GetComponent<EnemyAnimator>().enabled = false;
            Debug.Log("HÄÄÄÄÄÄÄÄÄÄ");
        }

    }

    public override void OnStartLocalPlayer()
    {
        if (!isAnimating) return;

        base.OnStartLocalPlayer();
        GetComponent<NetworkAnimator>().SetParameterAutoSend(0, true);
        GetComponent<NetworkAnimator>().SetParameterAutoSend(1, true);
        GetComponent<NetworkAnimator>().SetParameterAutoSend(2, true);
    }

    public override void PreStartClient()
    {
        if (!isAnimating) return;

        base.PreStartClient();
        GetComponent<NetworkAnimator>().SetParameterAutoSend(0, true);
        GetComponent<NetworkAnimator>().SetParameterAutoSend(1, true);
        GetComponent<NetworkAnimator>().SetParameterAutoSend(2, true);
    }

    private void OnDestroy()
    {
        PlayerManager.instance.RemoveEnemy(gameObject);
    }
}
