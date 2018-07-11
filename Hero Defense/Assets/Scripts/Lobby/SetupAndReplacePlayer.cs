using UnityEngine;
using UnityEngine.Networking;
using Prototype.NetworkLobby;

public class SetupAndReplacePlayer : NetworkBehaviour {

    public GameObject playableCharacterMage;
    public GameObject playableCharacterGunslinger;


    [SyncVar]
    public string playerName;

    [SyncVar]
    public  int playerCharacter;


    void Update()
    {
        Debug.Log("playerName =" + playerName);

        if(connectionToClient.isReady)
        {
            CmdReplacePlayer();
        }        
    }

    [Command]
    public void CmdReplacePlayer()
    {
        GameObject go;

        switch ((CharacterEnum)playerCharacter)
        {
            case CharacterEnum.Gunslinger:
                go = Instantiate(playableCharacterGunslinger, transform.position, transform.rotation);
                break;
            case CharacterEnum.Mage:
                go = Instantiate(playableCharacterMage, transform.position, transform.rotation);
                break;
            default:
                go = Instantiate(playableCharacterGunslinger, transform.position, transform.rotation);
                break;
        }
         
        NetworkServer.Spawn(go);

        if(NetworkServer.ReplacePlayerForConnection(connectionToClient,go,playerControllerId))
        {
            NetworkServer.Destroy(this.gameObject);
        }
    }
}
