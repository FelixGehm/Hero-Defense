using UnityEngine;

public class CharacterStats : MonoBehaviour
{


    public HealthBarManager healthBarManager;



    private UIHealthBar uIHealthBar;


    void Awake()
    {
        CurrentHealth = maxHealth.GetValue();

        uIHealthBar = GameObject.Find("Canvas HUD").transform.Find("CharacterInfo").Find("UIHealthBar").GetComponent<UIHealthBar>();
    }

    //[HideInInspector]
    public bool isControlledByServer = false;

    private float currentHealth;
    public virtual float CurrentHealth
    {
        get
        {
            return currentHealth;
        }
        set
        {
            currentHealth = value;

            if (healthBarManager != null)
            {
                healthBarManager.CurrentHealth = value;

                int isLocalPlayer = string.Compare("LocalPlayer", gameObject.name.Substring(0, 10));

                if (uIHealthBar != null && !gameObject.CompareTag("Enemy") && isLocalPlayer == 1)
                {
                    uIHealthBar.CurrentHealth = value;
                }

                if (currentHealth <= 0)
                {
                    Die();
                }
            }
        }
    }

    public Stat maxHealth;

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




    //evtl zusätzlich die Art des Schadens übergeben
    /*
    * Sobald der Spieler im Netzwork ist, speichert er nicht mehr selbst sein Leben und sollte somit auch keine Änderungen daran vornehmen.
    * Der Server kümmert sich um Änderungen und Teilt diese den CharakterStats mit.
    */
    public void TakeDamage(float damage)
    {
        Debug.Log("TakeDamage: dmg: " + damage + " isControlledByServer = " + isControlledByServer);

        if (!isControlledByServer)
        {
            CurrentHealth -= CalcTakenDamage(damage);

            Debug.Log(transform.name + " takes " + damage + " damage");

            if (CurrentHealth <= 0)
                Die();
        }
    }


    /*
     * Da im NetworkCharakter Controller der Server bei Mulitplayer das aktuelle Laben hält, 
     * habe ich diese Methode eingeführt, die auf Basis der eigenen Stats den angerichteten Schaden ausrechnet.
     * So kann sowohl lokal, als auch außerhalb der zugefügte Schaden berechnet werden.     * 
     */
    public float CalcTakenDamage(float incomingDamage)
    {
        float damage = incomingDamage;
        damage -= armor.GetValue();
        damage = Mathf.Max(damage, 0);
        return damage;
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
