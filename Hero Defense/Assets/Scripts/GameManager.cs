using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    #region Singleton
    public static GameManager instance;
    void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one intance of GameManager!");
            return;
        }
        instance = this;
    }
    #endregion

    public GameObject nexus;

    // Use this for initialization
    void Start()
    {
        nexus.GetComponent<NexusStats>().OnNexusDestroyed += OnGameEnded;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnGameEnded()
    {
        Debug.Log("Game has ended.");
        //Destroy(nexus);
        //hier implementieren, was nach Spielende geschehen soll.
    }
}
