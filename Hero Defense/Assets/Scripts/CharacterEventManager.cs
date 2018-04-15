using UnityEngine;
using UnityEngine.Networking;

public class CharacterEventManager : NetworkBehaviour
{
    public enum eventType { teleport, one, two, three, four, cancel }

    #region Actions
    /// <summary>
    /// Diverse Actions, bei denen via Delegates diverse Sachen wie Animationen, Fähigkeiten etc. angemeldet werden.
    /// </summary>

    public event System.Action OnTeleport;

    public event System.Action OnAbilityOne;

    public event System.Action OnAbilityTwo;

    public event System.Action OnAbilityThree;

    public event System.Action OnAbilityFour;

    public event System.Action OnCastCancel;

    #endregion

    
    private bool isSilenced = false;
    public bool IsSilenced {
        get
        {
            return isSilenced;
        }
        set
        {
            if(OnCastCancel != null && value == true)
            {
                OnCastCancel();
            }

            isSilenced = value;
        }
    }


    #region Methoden zum Triggern der Actions
    /// <summary>
    /// Sobald eine der folgenden Methoden aufgerufen wird, werden alle angemeldeten Delegates für die entsprechende Action ausgeführt.
    /// </summary>
    public void Teleport()
    {
        Debug.Log("Teleport():");

        TransmitEvent(CharacterEventManager.eventType.teleport);

        if (OnTeleport != null && !isSilenced)
        {
            OnTeleport();
        }
    }

    public void AbilityOne()
    {
        Debug.Log("AbilityOne():");

        TransmitEvent(CharacterEventManager.eventType.one);

        if (OnAbilityOne != null && !isSilenced)
        {
            OnAbilityOne();
        }
    }

    public void AbilityTwo()
    {
        Debug.Log("AbilityTwo():");

        TransmitEvent(CharacterEventManager.eventType.two);

        if (OnAbilityTwo != null && !isSilenced)
        {
            OnAbilityTwo();
        }
    }

    public void AbilityThree()
    {
        Debug.Log("AbilityThree():");

        TransmitEvent(CharacterEventManager.eventType.three);

        if (OnAbilityThree != null && !isSilenced)
        {
            OnAbilityThree();
        }
    }

    public void AbilityFour()
    {
        Debug.Log("AbilityFour():");

        TransmitEvent(CharacterEventManager.eventType.four);

        if (OnAbilityFour != null && !isSilenced)
        {
            OnAbilityFour();
        }
    }

    public void CastCancel()
    {
        Debug.Log("CastCancel():");

        TransmitEvent(CharacterEventManager.eventType.cancel);

        if (OnCastCancel != null)
        {
            OnCastCancel();
        }
    }

    #endregion


    #region Netzwerk


    /// <summary>
    /// Command-Methoden bilden den Kommunikationsweg von den Clients zu dem Server.
    /// Sobald ein Client ein Command aufruft, wird der Code in dem Command BEI DEM SERVER SELBST ausgeführt. 
    /// Die Clients schicken also nur eine Aufforderung zum Ausführen des Codes an den Server.
    /// 
    /// In diesem Beispiel wird das Command dazu verwendet, um die verschiedenen Events bei dem Server zu triggern.
    /// </summary>
    /// <param name="type"></param>
    [Command]
    void CmdProvideEventToServer(CharacterEventManager.eventType type)
    {
        switch (type)
        {
            case CharacterEventManager.eventType.one:
                AbilityOne();
                break;
            case CharacterEventManager.eventType.two:
                AbilityTwo();
                break;
            case CharacterEventManager.eventType.three:
                AbilityThree();
                break;
            case CharacterEventManager.eventType.four:
                AbilityFour();
                break;
            case CharacterEventManager.eventType.teleport:
                Teleport();
                break;
        }

    }


    /// <summary>
    /// ClientCallBack unterdrückt laut Unity-Dokumentation Warnungen im Editor wenn ein Command aufgerufen wird...
    /// 
    /// Mehr macht das Attribut (glaube ich) nicht... Die Commands werden also nur auf diesem Umweg ausgeführt, um den Editor nicht vollzuspamen!
    /// 
    /// </summary>
    /// <param name="type"></param>
    [ClientCallback]
    public void TransmitEvent(CharacterEventManager.eventType type)
    {
        if (!isServer)
        {
            CmdProvideEventToServer(type);
        }
    }
    #endregion



}
