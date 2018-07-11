using UnityEngine;
using Prototype.NetworkLobby;
using UnityEngine.Networking;


public class NetworkLobbyHook : LobbyHook
{
    public override void OnLobbyServerSceneLoadedForPlayer(NetworkManager manager, 
                                                           GameObject lobbyPlayer, GameObject gamePlayer)
    {        
        base.OnLobbyServerSceneLoadedForPlayer(manager, lobbyPlayer, gamePlayer);

        LobbyPlayer lobby = lobbyPlayer.GetComponent<LobbyPlayer>();
        SetupAndReplacePlayer localPlayer = gamePlayer.GetComponent<SetupAndReplacePlayer>();

        

        localPlayer.playerName = lobby.playerName;
        localPlayer.playerCharacter = (int)lobby.playerChar;        
    }




}
