using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarManager : MonoBehaviour {

    private Image foreGround;

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
            Debug.Log("Set MaxHealth()");
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




    void Awake ()
    {
        cam = Camera.main;

        foreGround = transform.GetChild(0).GetChild(0).GetComponent<Image>();
        //Debug.Log(foreGround.name);

        stats = transform.parent.parent.GetComponent<CharacterStats>();

        maxHealth = stats.maxHealth.GetValue();
        currentHealth = maxHealth;
	}

    private void Update()
    {

        transform.rotation = cam.transform.rotation;

    }




}
