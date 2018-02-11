using UnityEngine;

public class CharacterStats : MonoBehaviour {
    public Stat maxHealth;
    //nur in dieser klasse setzbar, aber von überall abrufbar
    public int currentHealth { get; private set; }

    public Stat damage;
    public Stat attackSpeed;

    /*
    public Stat critChance;
    public Stat critDamage;
    */

    public Stat armor;

    //TODO: Elementar Resistenzen, Elementar Schadensarten

    //public Stat moveSpeed;

    void Awake()
    {
        currentHealth = maxHealth.GetValue();
    }

    //evtl zusätzlich die Art des Schadens übergeben
    public void TakeDamage (int damage)
    {
        damage -= armor.GetValue();
        damage = Mathf.Max(damage, 0);

        currentHealth -= damage;
        Debug.Log(transform.name + " takes " + damage + " damage");

        if (currentHealth <= 0)
            Die();
    }

    public virtual void Die()
    {
        Debug.Log(transform.name + " died.");
    }
}
