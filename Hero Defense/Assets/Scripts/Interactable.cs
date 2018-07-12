using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour {

    public float interactionRadius = 1;
    public Transform interactionTransform;

    [HideInInspector]
    public bool isFocus = false;
    Transform player;

    bool hasInteracted = false;

    public GameObject haloInstance;

    public event System.Action OnDefocus;

    void Awake()
    {

    }

    public virtual void Interact()
    {
        Debug.Log("Interacting with " + transform.name);
    }

    private void Update()
    {
        if (isFocus && !hasInteracted)
        {
            float distance = Vector3.Distance(player.position, interactionTransform.position);
            if (distance <= interactionRadius){
                Interact();
                hasInteracted = true;
            }
        }
    }

    
    public void OnFocused(Transform playerTransform)
    {
        isFocus = true;
        player = playerTransform;
        hasInteracted = false;

        haloInstance.SetActive(true);

        
    }

    public void OnLeftClick()
    {        
        haloInstance.SetActive(true);
    }

    public void OnDefocused()
    {
        isFocus = false;
        player = null;
        hasInteracted = false;

        haloInstance.SetActive(false);

        OnDefocus?.Invoke();
    }

    private void OnDrawGizmosSelected()
    {
        if (interactionTransform == null)
            interactionTransform = transform;

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(interactionTransform.position, interactionRadius);
    }
}
