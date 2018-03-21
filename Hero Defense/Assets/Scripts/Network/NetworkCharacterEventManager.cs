using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

[RequireComponent(typeof(CharacterEventManager))]
public class NetworkCharacterEventManager : NetworkBehaviour
{

    CharacterEventManager characterEventManager;

    // Use this for initialization
    void Start()
    {
        characterEventManager = GetComponent<CharacterEventManager>();
    }


    [Command]
    void CmdProvideEventToServer(CharacterEventManager.eventType type)
    {
        switch (type)
        {
            case CharacterEventManager.eventType.one:
                characterEventManager.AbilityOne();
                break;
            case CharacterEventManager.eventType.two:
                characterEventManager.AbilityTwo();
                break;
            case CharacterEventManager.eventType.three:
                characterEventManager.AbilityThree();
                break;
            case CharacterEventManager.eventType.four:
                characterEventManager.AbilityFour();
                break;
            case CharacterEventManager.eventType.teleport:
                characterEventManager.Teleport();
                break;
        }
        //characterEventManager.AbilityOne();
    }

    [ClientCallback]
    public void TransmitEvent(CharacterEventManager.eventType type)
    {
        if (!isServer)
        {
            CmdProvideEventToServer(type);
        }
    }

}
