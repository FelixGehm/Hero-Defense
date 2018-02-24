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
    public GameObject player;

    public GameObject[] players;
    
    public GameObject nexus;

    private void Start()
    {
        players = new GameObject[3]; // Maximal 3 Spieler pro Lobby

    }
    
    public void RegisterPlayer(GameObject _player)
    {
        for ( int i = 0; i< players.Length; i++)
        {
            if (players[i] == null)
            {
                players[i] = _player;
                return;
            }
        }

    }

    public void RemovePlayer(GameObject _player)
    {
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i] == _player)
            {
                players[i] = null;
                return;
            }
        }
    }


    public void KillPlayer()
    {
        //hier implementieren, was passieren soll, wenn spieler stirbt
    }
}
