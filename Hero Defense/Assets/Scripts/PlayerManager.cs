﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.Networking;

//Hält eine Referenz auf den Spieler. So müssen nicht bei jeden Enemy Spawn alle Objekte nach dem Spieler durchsucht werden, sonder der Spieler kann über diese Klasse abgerufen werden.

    //TODO: Rename in "EntityManager"

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

    

    public GameObject[] players;

    public List<GameObject> enemies;
    
    public GameObject nexus;

    private void Start()
    {
        players = new GameObject[4]; // Maximal 4 Spieler pro Lobby

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

    public void RegisterEnemy(GameObject enemy)
    {
        enemies.Add(enemy);
    }

    public void RemoveEnemy(GameObject enemy)
    {
        enemies.Remove(enemy);
    }


    public void KillPlayer()
    {
        //hier implementieren, was passieren soll, wenn spieler stirbt
    }

    private void OnDestroy()
    {
        PlayerManager.instance.RegisterEnemy(gameObject);
    }


}
