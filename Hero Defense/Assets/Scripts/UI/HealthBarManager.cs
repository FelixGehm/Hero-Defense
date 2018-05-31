using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarManager : MonoBehaviour {

    public Image foreGround;

    private CharacterStats stats;

    private Camera cam;


    private float maxHealth;
    public float MaxHealth
    {

        get
        {
            return maxHealth;
        }
        set
        {
           // Debug.Log("Set MaxHealth() to " + value);
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
            //Debug.Log("Set CurrentHealth()");
            currentHealth = value;
            foreGround.fillAmount = currentHealth/maxHealth;
        }
    }




    void Start ()
    {
        stats = transform.parent.parent.GetComponent<CharacterStats>();

        //Debug.Log(stats);

        maxHealth = stats.maxHealth.GetValue();
        currentHealth = maxHealth;
	}

    private void Update()
    {
        /*
        if (cam != null)
        {
            transform.rotation = cam.transform.rotation;
        } else
        {
            cam = Camera.main;
        }*/
        

    }




}
