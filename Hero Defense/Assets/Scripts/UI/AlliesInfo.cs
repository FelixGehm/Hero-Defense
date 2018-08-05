using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class AlliesInfo : MonoBehaviour
{

    public PlayerManager playerManager;
    public UIHealthBar[] allieHealthBar;

    private int registeredPlayers = -1;

    void Awake()
    {
        playerManager.OnPlayerRegistered += AddPlayerInfoToTrack;
    }


    public void AddPlayerInfoToTrack(GameObject playerGO)
    {
        if( playerGO.GetComponent<NetworkIdentity>().isLocalPlayer)
        {
            return;
        }

        CharacterStats playerStats = playerGO.GetComponent<CharacterStats>();

        allieHealthBar[registeredPlayers + 1].RegisterCharacterStats(playerStats);
        registeredPlayers++;
    }

}
