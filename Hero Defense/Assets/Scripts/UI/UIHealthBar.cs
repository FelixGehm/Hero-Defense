﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIHealthBar : MonoBehaviour
{

    public Image foreGround;
    public CharacterStats stats;

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
            foreGround.fillAmount = currentHealth / maxHealth;
        }
    }

    // Use this for initialization
    void Start()
    {
        foreGround.fillAmount = 1.0f;
    }

    public void RegisterPlayerStats(CharacterStats _stats)
    {
        stats = _stats;
        maxHealth = stats.maxHealth.GetValue();
        currentHealth = maxHealth;
    }
}
