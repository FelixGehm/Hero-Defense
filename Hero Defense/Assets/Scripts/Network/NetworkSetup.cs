using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkSetup : NetworkBehaviour {

    [SerializeField]
    protected Behaviour[] componentsToDisable;

    void Start()
    {
        DisableComponents();
    }

    protected virtual void DisableComponents()
    {
        if (!isLocalPlayer)
        {
            for (int i = 0; i < componentsToDisable.Length; i++)
            {
                componentsToDisable[i].enabled = false;
            }
        }
    }
}
