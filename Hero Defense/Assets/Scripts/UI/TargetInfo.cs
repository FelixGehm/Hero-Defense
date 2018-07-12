using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TargetInfo : MonoBehaviour
{
    public UIHealthBar targetHealthBar;

    public GameObject visuals;

    PlayerController localPlayerController;

    Interactable currentTarget;


    bool isReady = false;
    private void Update()
    {
        if (!isReady)
        {
            GameObject localPlayer = GameObject.Find("_Game").GetComponent<PlayerManager>().GetLocalPlayer();

            if (localPlayer != null)
            {
                localPlayerController = localPlayer.GetComponent<PlayerController>();
                localPlayerController.focusChanged += OnTargetChanged;  // es leben Delegates!

                isReady = true;
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, 100))
                {
                    if (hit.collider.tag == "Enemy")
                    {
                        Interactable newFocus = hit.transform.GetComponent<Interactable>();

                        if (newFocus != currentTarget)
                        {
                            OnTargetChanged(newFocus);
                            newFocus.OnLeftClick();
                        }
                    }
                }
            }
            if (Input.GetMouseButtonDown(1))
            {
                if (currentTarget != null)
                {
                    currentTarget.OnDefocused();
                    currentTarget = null;
                    visuals.SetActive(false);
                }
            }
        }

        if (currentTarget == null)
        {
            visuals.SetActive(false);
        }
    }

    public void OnTargetChanged(Interactable newTarget)
    {
        //Debug.Log("OnTargetChanged()");

        if (newTarget != null)
        {
            visuals.SetActive(true);

            if (currentTarget != null)
            {
                currentTarget.OnDefocused();
            }

            currentTarget = newTarget;

            CharacterStats targetStats = newTarget.interactionTransform.GetComponent<CharacterStats>();
            targetHealthBar.RegisterCharacterStats(targetStats);
        }
    }
}
