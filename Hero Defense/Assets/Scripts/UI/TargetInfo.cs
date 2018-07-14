using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TargetInfo : MonoBehaviour
{
    public UIHealthBar targetHealthBar;

    public GameObject visuals;

    // UI Refferences:
    public Text physicalText;
    public Text armorText;
    public Text magicText;
    public Text resistanceText;
    public Text attackSpeedText;
    public Text movementSpeedText;
    public Text healthText;

    Interactable currentTarget;
    CharacterStats targetStats;

        
    private void Update()
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
                    }
                }
            }
        }

        if (currentTarget != null)
        {
            SetTargetCurrentHealth();
        }
        else 
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
                currentTarget.haloInstance.SetActive(false);
            }

            currentTarget = newTarget;
            newTarget.haloInstance.SetActive(true);

            targetStats = newTarget.interactionTransform.GetComponent<CharacterStats>();
            targetHealthBar.RegisterCharacterStats(targetStats);

            SetTargetStatsInUI(targetStats);
        }
    }

    private void SetTargetStatsInUI(CharacterStats targetStats)
    {
        physicalText.text = targetStats.physicalDamage.GetValue().ToString();
        armorText.text = targetStats.armor.GetValue().ToString();
        attackSpeedText.text = targetStats.attackSpeed.GetValue().ToString();

        magicText.text = targetStats.magicDamage.GetValue().ToString();
        resistanceText.text = targetStats.magicResistance.GetValue().ToString();
        movementSpeedText.text = targetStats.moveSpeed.GetValue().ToString();
    }

    private void SetTargetCurrentHealth()
    {
        string tmp = targetStats.SyncedCurrentHealth + "/" + targetStats.maxHealth.GetValue();

        healthText.text = tmp;
    }

}
