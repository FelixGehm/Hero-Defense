using UnityEngine;

public class DestroyAfterSeconds : MonoBehaviour
{

    public float secondsToDestroy;

    void Start()
    {
        Destroy(gameObject, secondsToDestroy);
    }


}

