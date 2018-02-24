using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkSetupEnemy : NetworkSetup {

    void Start()
    {

        base.DisableComponents();

        if (isServer)
        {
            GetComponent<EnemyController>().SetupEnemy();
            
            // Register Enemy in PlayerManager ?? 
            //PlayerManager.instance.RegisterPlayer(transform.gameObject);
        }

    }

    protected override void DisableComponents()
    {
        if (!isServer)
        {
            for (int i = 0; i < componentsToDisable.Length; i++)
            {
                componentsToDisable[i].enabled = false;
            }
        }
    }

    private void OnDestroy()
    {
        // Remove Enemy from PlayerManager ??
        //PlayerManager.instance.RemovePlayer(transform.gameObject);
    }
}
