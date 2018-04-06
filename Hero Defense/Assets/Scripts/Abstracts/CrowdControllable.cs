using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CrowdControllable : MonoBehaviour
{
    enum Status { _default, taunted, stunned}

    /// <summary>
    /// 
    /// </summary>
    /// <param name="tauntTarget"> the Transform of the Object who taunted me </param>
    /// <param name="duration"> in seconds </param>
    abstract public void GetTaunted(Transform tauntTarget, float duration);

    /// <summary>
    /// Selfexplaining??
    /// </summary>
    /// <param name="duration"></param>
    /// <returns></returns>
    abstract public IEnumerator EndTauntAfter(float duration);      // überhaupt notwendig?? alternativ: GetTaunted rückgabe IEnumerator und beendet selbst den zustand. besser?

    /// <summary>
    /// 
    /// </summary>
    /// <param name="duration"> in seconds </param>
    abstract public void GetStunned(float duration);
}
