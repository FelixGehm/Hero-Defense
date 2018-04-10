using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CrowdControllable : MonoBehaviour
{
    public enum Status { _default, taunted, stunned, silenced, blind, crippled, bleeding}

    //protected Status myStatus = Status._default;

    protected List<Status> myStatuses;    // Jedes mal, wenn ein Statuseffekt auf mich angewandt wird, muss die Klasse, die diese Abstrakte Klasse implementiert,
                                        // dafür sorgen, dass er der Liste hinzugefügt und beim AUslaufen des Effekts wieder entfernt wird.


    public virtual void Start()
    {
        myStatuses = new List<Status>();
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
    /// <param name="livePerTick"></param>
    /// <returns></returns>
    abstract public IEnumerator GetBleedingWound(int duration, float livePerTick);
}
