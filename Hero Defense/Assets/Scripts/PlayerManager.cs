using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.Networking;

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

    [SerializeField]
    public GameObject[] player;
    
    public GameObject nexus;

    private void Start()
    {
        player = new GameObject[3]; // Maximal 3 Spieler pro Lobby

    }
    
    public void RegisterPlayer(GameObject _player)
    {
        for ( int i = 0; i< player.Length; i++)
        {
            if (player[i] == null)
            {
                player[i] = _player;
                return;
            }
        }

    }

    public void RemovePlayer(GameObject _player)
    {
        for (int i = 0; i < player.Length; i++)
        {
            if (player[i] == _player)
            {
                player[i] = null;
                return;
            }
        }
    }


    public void KillPlayer()
    {
        //hier implementieren, was passieren soll, wenn spieler stirbt
    }
}
