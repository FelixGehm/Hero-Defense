using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    public Stat maxHealth;
    //nur in dieser klasse setzbar, aber von überall abrufbar



    //[SyncVar]
    private float syncCurrentHealth;            // Server aktualisiert Leben für alle Clients

    //private float currentLocalHealth;

    public HealthBarManager healthBarManager;
    public float CurrentHealth
    {
        get
        {
            return syncCurrentHealth;
        }
        private set
        {
            syncCurrentHealth = value;

            //TransmitHealth();




            if (healthBarManager != null)
            {
                healthBarManager.CurrentHealth = value;
            }

        }
    }

    /*
    [Command]
    void CmdProvideHealthToServer(float newCurrentHealth)
    {
        syncCurrentHealth = newCurrentHealth;
    }
    */

    /*
// Client teilt Server Änderung mit
[ClientCallback]
void TransmitHealth()
{
    CmdProvideHealthToServer(syncCurrentHealth);
}
*/


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
