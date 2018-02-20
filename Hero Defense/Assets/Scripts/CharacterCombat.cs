using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterStats))]
public class CharacterCombat : MonoBehaviour
{
    //to match damage output with animation
    public float attackDelay = 0.2f;

    private float attackSpeed;
    private float attackCooldown;

    public event System.Action OnAttack;

    private HealthBarManager healthBar;

    CharacterStats myStats;

    void Start()
    {
        myStats = GetComponent<CharacterStats>();

        healthBar = transform.Find("GFX").Find("HealthBar").GetComponent<HealthBarManager>();

        healthBar.MaxHealth = myStats.maxHealth.GetValue();
    }

    void Update()
    {
        attackCooldown -= Time.deltaTime;
    }

    public void Attack(CharacterStats targetStats)
    {
        if (attackCooldown <= 0)
        {
            attackSpeed = (float)myStats.attackSpeed.GetValue() / 100;
            StartCoroutine(DoDamage(targetStats, attackDelay));

            if (OnAttack != null)
                OnAttack();

            attackCooldown = 1 / attackSpeed;
        }
    }

    IEnumerator DoDamage(CharacterStats stats, float delay)
    {
        yield return new WaitForSeconds(delay);
        stats.TakeDamage(myStats.damage.GetValue());

        healthBar.CurrentHealth = myStats.currentHealth;
    }
}
