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
    abstract protected IEnumerator GetTauntedCo(Transform tauntTarget, float duration);
 
    public void GetTaunted(Transform tauntTarget, float duration)
    {
        StartCoroutine(GetTauntedCo(tauntTarget, duration));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="duration"> in seconds </param>
    abstract protected IEnumerator GetStunnedCo(float duration);

    public void GetStunned(float duration)
    {
        StartCoroutine(GetStunnedCo(duration));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="duration"> in seconds </param>
    abstract protected IEnumerator GetSilencedCo(float duration);

    public void GetSileced(float duration)
    {
        StartCoroutine(GetSilencedCo(duration));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="duration"> in seconds </param>
    abstract protected IEnumerator GetBlindedCo(float duration);
    
    public void GetBlinded(float duration)
    {
        StartCoroutine(GetBlindedCo(duration));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="duration"> in seconds </param>
    /// <param name="percent"></param>
    /// <returns></returns>
    abstract protected IEnumerator GetCrippledCo(float duration, float percent);

    public void GetCrippled(float duration, float percent)
    {
        StartCoroutine(GetCrippledCo(duration, percent));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="duration"></param>
    /// <param name="percentPerTick"></param>
    /// <returns></returns>
    abstract protected IEnumerator GetBleedingWoundCo(int duration, float percentPerTick);

    public void GetBleedingWound(int durationInTicks, float percentPerTick)
    {
        StartCoroutine(GetBleedingWoundCo(durationInTicks, percentPerTick));
    }
}
