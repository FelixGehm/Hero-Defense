using UnityEngine;

public class CharacterEventManager : MonoBehaviour
{
    public event System.Action OnTeleport;

    public event System.Action OnAbilityOne;

    public event System.Action OnAbilityTwo;

    public event System.Action OnAbilityThree;

    public event System.Action OnAbilityFour;

    public event System.Action OnCastCancel;


    public void Teleport()
    {
        Debug.Log("Teleport():");

        if (OnTeleport != null)
        {
            OnTeleport();
        }
    }

    public void AbilityOne()
    {
        Debug.Log("AbilityOne():");

        if (OnAbilityOne != null)
        {
            OnAbilityOne();
        }
    }

    public void AbilityTwo()
    {
        Debug.Log("AbilityTwo():");

        if (OnAbilityTwo != null)
        {
            OnAbilityTwo();
        }
    }

    public void AbilityThree()
    {
        Debug.Log("AbilityThree():");

        if (OnAbilityThree != null)
        {
            OnAbilityThree();
        }
    }

    public void AbilityFour()
    {
        Debug.Log("AbilityFour():");

        if (OnAbilityFour != null)
        {
            OnAbilityFour();
        }
    }

    public void CastCancel()
    {
        Debug.Log("CastCancel():");

        if (OnCastCancel != null)
        {
            OnCastCancel();
        }
    }



}
