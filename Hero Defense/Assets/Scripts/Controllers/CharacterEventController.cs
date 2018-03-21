using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(CharacterEventManager))]
public class CharacterEventController : MonoBehaviour
{

    private CharacterEventManager characterEventManager;
    private NetworkCharacterEventManager networkCharacterEventManager;


    public KeyCode teleportKey = KeyCode.B;

    public KeyCode abilityOneKey = KeyCode.Q;
    public KeyCode abilityTwoKey = KeyCode.W;
    public KeyCode abilityThreeKey = KeyCode.E;
    public KeyCode abilityFourKey = KeyCode.R;

    void Start()
    {
        characterEventManager = GetComponent<CharacterEventManager>();

        if (GetComponent<NetworkCharacterEventManager>() != null)
        {
            networkCharacterEventManager = GetComponent<NetworkCharacterEventManager>();
        }

    }

    void Update()
    {
        if (Input.GetKeyDown(teleportKey))
        {
            characterEventManager.Teleport();
            networkCharacterEventManager.TransmitEvent(CharacterEventManager.eventType.teleport);
        }

        if (Input.GetKeyDown(abilityOneKey))
        {
            characterEventManager.AbilityOne();
            networkCharacterEventManager.TransmitEvent(CharacterEventManager.eventType.one);
        }

        if (Input.GetKeyDown(abilityTwoKey))
        {
            networkCharacterEventManager.TransmitEvent(CharacterEventManager.eventType.two);
            characterEventManager.AbilityTwo();
        }

        if (Input.GetKeyDown(abilityThreeKey))
        {
            networkCharacterEventManager.TransmitEvent(CharacterEventManager.eventType.three);
            characterEventManager.AbilityThree();
        }

        if (Input.GetKeyDown(abilityFourKey))
        {
            networkCharacterEventManager.TransmitEvent(CharacterEventManager.eventType.four);
            characterEventManager.AbilityFour();
        }
    }

}
