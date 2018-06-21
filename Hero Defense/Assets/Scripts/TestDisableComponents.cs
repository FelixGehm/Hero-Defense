using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class TestDisableComponents : NetworkBehaviour
{
    public Behaviour[] componentsToDisable;
    // Use this for initialization
    void Awake()
    {
        if (!isServer)
        {
            for (int i = 0; i < componentsToDisable.Length; i++)
            {
                componentsToDisable[i].enabled = false;
            }
        }
    }
}
