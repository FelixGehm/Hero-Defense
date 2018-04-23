using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CrowdControllable : MonoBehaviour
{
    public enum Status { _default, taunted, stunned, silenced, blind, crippled, bleeding}

    //protected Status myStatus = Status._default;

    public ObservableStatusCollection myStatuses;    // Jedes mal, wenn ein Statuseffekt auf mich angewandt wird, muss die Klasse, die diese Abstrakte Klasse implementiert,
                                                      // dafür sorgen, dass er der Liste hinzugefügt und beim Auslaufen des Effekts wieder entfernt wird.

    public ObservableStatusCollection GetObservableStatusCollection()
    {
        return myStatuses;
    }


    public virtual void Awake()
    {
        myStatuses = new ObservableStatusCollection();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="tauntTarget"> the Transform of the Object who taunted me </param>
    /// <param name="duration"> in seconds </param>
    abstract public IEnumerator GetTaunted(Transform tauntTarget, float duration);
 
    /// <summary>
    /// 
    /// </summary>
    /// <param name="duration"> in seconds </param>
    abstract public IEnumerator GetStunned(float duration);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="duration"> in seconds </param>
    abstract public IEnumerator GetSilenced(float duration);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="duration"> in seconds </param>
    abstract public IEnumerator GetBlinded(float duration);
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="duration"> in seconds </param>
    /// <param name="percent"></param>
    /// <returns></returns>
    abstract public IEnumerator GetCrippled(float duration, float percent);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="duration"></param>
    /// <param name="percentPerTick"></param>
    /// <returns></returns>
    abstract public IEnumerator GetBleedingWound(int duration, float percentPerTick);
}
