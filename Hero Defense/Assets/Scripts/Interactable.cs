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

    private GameObject focusHaloPrefab;
    private GameObject haloInstance; 

    void Awake()
    {
        // Funktioniert so leider nicht :(
        focusHaloPrefab = (GameObject)Resources.Load("Focus_Sprite", typeof(GameObject));
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

        haloInstance = Instantiate(focusHaloPrefab, transform.Find("GFX"), false);
    }

    public void OnDefocused()
    {
        isFocus = false;
        player = null;
        hasInteracted = false;

        Destroy(haloInstance, 0.2f);
    }

    private void OnDrawGizmosSelected()
    {
        if (interactionTransform == null)
            interactionTransform = transform;

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(interactionTransform.position, interactionRadius);
    }
}
