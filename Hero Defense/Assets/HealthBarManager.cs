using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarManager : MonoBehaviour {

    private Image foreGround;

    private CharacterStats stats;


    private float maxHealth;
    public float MaxHealth
    {

        get
        {
            return maxHealth;
        }
        set
        {
            maxHealth = value;
            foreGround.fillAmount = currentHealth / maxHealth;
        }
    }

    private float currentHealth;
    public float CurrentHealth
    {

        get
        {
            return currentHealth;
        }
        set
        {
            currentHealth = value;
            foreGround.fillAmount = currentHealth/maxHealth;
        }
    }




    void Awake ()
    {
        foreGround = transform.GetChild(0).GetChild(0).GetComponent<Image>();

        stats = transform.parent.parent.GetComponent<CharacterStats>();

        maxHealth = stats.maxHealth.GetValue();
        currentHealth = maxHealth;
	}




}
