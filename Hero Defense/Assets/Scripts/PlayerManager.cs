using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Hält eine Referenz auf den Spieler. So müssen nicht bei jeden Enemy Spawn alle Objekte nach dem Spieler durchsucht werden, sonder der Spieler kann über diese Klasse abgerufen werden.

public class PlayerManager : MonoBehaviour {


    #region Singleton
    public static PlayerManager instance;
    void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one intance of PlayerManager!");
            return;
        }
        instance = this;
    }
    #endregion

    public GameObject player;
    public GameObject nexus;


    public void KillPlayer()
    {
        //hier implementieren, was passieren soll, wenn spieler stirbt
    }
}
