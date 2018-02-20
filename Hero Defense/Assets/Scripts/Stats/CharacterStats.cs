using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    public Stat maxHealth;
    //nur in dieser klasse setzbar, aber von überall abrufbar
    private float currentHealth;
    public HealthBarManager healthBarManager;
    public float CurrentHealth
    {
        get
        {
            return currentHealth;
        }
        private set
        {
            currentHealth = value;

            if (healthBarManager != null)
            {
                healthBarManager.CurrentHealth = value;
            }

        }
    }

    public Stat damage;
    public Stat attackSpeed;

    public Stat attackRange;


    /*
    public Stat critChance;
    public Stat critDamage;
    */

    public Stat armor;

    //TODO: Elementar Resistenzen, Elementar Schadensarten

    //public Stat moveSpeed;

    void Awake()
    {
        CurrentHealth = maxHealth.GetValue();
    }

    //evtl zusätzlich die Art des Schadens übergeben
    public void TakeDamage(float damage)
    {
        damage -= armor.GetValue();
        damage = Mathf.Max(damage, 0);

        CurrentHealth -= damage;
        Debug.Log(transform.name + " takes " + damage + " damage");

        if (CurrentHealth <= 0)
            Die();
    }

    public virtual void Die()
    {
        Debug.Log(transform.name + " died.");

    }


    private void OnDrawGizmosSelected()
    {
        //Draw AttackRange

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, (float)attackRange.GetValue());
    }
}
