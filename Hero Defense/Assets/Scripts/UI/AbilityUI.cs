using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AbilityUI : MonoBehaviour
{

    private AbilityBasic ability;
    public GameObject activeRing;
    public Image cdImg;
    public Image abilityImg;
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (ability == null) return;

        cdImg.fillAmount = ability.currentCooldown / ability.abilityCooldown;
    }

    public void RegisterAbilityToUI(AbilityBasic ability)
    {
        this.ability = ability;
    }

    public void RegisterAbilityToUI(AbilityBasic ability, Sprite abilitySprite)
    {
        this.ability = ability;
        if (abilitySprite != null)
        {
            cdImg.sprite = abilitySprite;
            abilityImg.sprite = abilitySprite;
        }

    }

    public void SetAbilityActive(bool a)
    {
        if (activeRing == null) return;

        if (a)
        {
            activeRing.SetActive(true);
        }
        else
        {
            activeRing.SetActive(false);
        }
    }
}
