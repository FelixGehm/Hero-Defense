using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(CharacterEventManager))]
public class CharacterEventController : MonoBehaviour
{

    private CharacterEventManager characterEventManager;
  
    public KeyCode teleportKey = KeyCode.B;

    public KeyCode abilityOneKey = KeyCode.Q;
    public KeyCode abilityTwoKey = KeyCode.W;
    public KeyCode abilityThreeKey = KeyCode.E;
    public KeyCode abilityFourKey = KeyCode.R;

    void Start()
    {
        characterEventManager = GetComponent<CharacterEventManager>();        
    }

    void Update()
    {
        if (Input.GetKeyDown(teleportKey))
        {
            characterEventManager.Teleport();
        }

        if (Input.GetKeyDown(abilityOneKey))
        {
            characterEventManager.AbilityOne();
        }

        if (Input.GetKeyDown(abilityTwoKey))
        {
            characterEventManager.AbilityTwo();
        }

        if (Input.GetKeyDown(abilityThreeKey))
        {
            characterEventManager.AbilityThree();
        }

        if (Input.GetKeyDown(abilityFourKey))
        {
            characterEventManager.AbilityFour();
        }
    }

}
