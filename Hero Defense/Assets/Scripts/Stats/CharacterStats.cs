using UnityEngine;
using UnityEngine.Networking;


/// <summary>
/// Muss von NetworkBehaviour erben, da auf diverse Variablen daraus zugegriffen wird, und mit SyncVars gearbeitet wird.
/// </summary>
public class CharacterStats : NetworkBehaviour
{
    public HealthBarManager healthBarManager;
    protected UIHealthBar uIHealthBar;

    

    public virtual void Awake()
    {
        CurrentHealth = maxHealth.GetValue();

        isEnemy = gameObject.CompareTag("Enemy");
    }

    protected bool isEnemy;


    /// <summary>
    /// Bei SyncVars kann man sogenannte Hooks anhängen. Das sind Methoden, die ausgeführt werden, sobald der Server die aktuallisierte Variable gesendet hat. 
    /// Sobald der Server den Wert für eine SyncVar bei den Clients ändert, wird dieser Hook dann ausgeführt.
    /// In diesem Fall wird dies angewandt, umd den Property, der auf die Syncvar verweist ebenfalls zu aktuallisieren.
    /// </summary>
    [SyncVar(hook = "OnChangeHealth")]
    private float syncedCurrentHealth;

    public virtual float CurrentHealth  //TODO: Das kann doch einfach in PlayerStats überschrieben werden. Dann wird das etwas übersichtlicher
    {
        get
        {
            return syncedCurrentHealth;
        }
        set
        {
            //Debug.Log("Health Changed to " + value);
            if (value > maxHealth.GetValue())
            {
                value = maxHealth.GetValue();
            }

            syncedCurrentHealth = value;

            if (healthBarManager != null)
            {
                healthBarManager.CurrentHealth = value;



                if (uIHealthBar != null && !isEnemy && isLocalPlayer)
                {
                    uIHealthBar.CurrentHealth = value;
                }

                if (syncedCurrentHealth <= 0)
                {
                    Die();
                    //Debug.Log("CurrentHealth: " + value);
                }
            }
        }
    }


    private void OnChangeHealth(float newHealth)
    {
        CurrentHealth = newHealth;      //Property CurrentHealth muss ebenfalls synchronisiert werden, damit die Healthbar angepasst wird.
    }


    public Stat maxHealth;


    protected static float timeBetweenTicksForHealthRegeneration = 1.0f;
    protected float currentTickCoolDown = timeBetweenTicksForHealthRegeneration;

    protected void Update()
    {
        if (isServer)
        {
            currentTickCoolDown -= Time.deltaTime;

            if (currentTickCoolDown <= 0)
            {
                currentTickCoolDown = timeBetweenTicksForHealthRegeneration;

                //Debug.Log("Time= "+Time.time);

                if (CurrentHealth > 0)
                    CurrentHealth += maxHealth.GetValue() * healthRegeneration.GetValue();
            }
        }

    }

    [Header("0.0 = 0% Health returned per tick, 1.0 = 100% Health returned per tick")]
    public Stat healthRegeneration;

    public Stat physicalDamage;
    public Stat armor;

    public Stat magicDamage;
    public Stat magicResistance;

    [Header("0.0 = 0% CritChance, 1.0 = 100% CritChance")]
    public Stat critChance;

    [Header("1.0 = 100% Damage, 1.5 = 150% Damage, 2 = 200% Damage")]
    public Stat critDamage;

    public Stat attackSpeed;
    public Stat attackRange;

    public Stat moveSpeed;

    // TODO: Stats im Netzwerk synchronisieren

    #region TrueDamage
    public void TakeTrueDamage(float tDamage)
    {
        if (!isServer)      // Ausschließlich der Server verursacht so Schaden.
        {
            return;
        }

        float damage = tDamage;
        damage = Mathf.Max(damage, 0);      // Check if damage is < 0, if yes -> set to 0

        CurrentHealth -= damage;
    }

    #endregion

    #region Heal
    public void TakeHeal(float healAmount)
    {
        if (!isServer) return;

        CurrentHealth += healAmount;
    }

    #endregion

    #region Physical
    public void TakePhysicalDamage(float pDamage)
    {
        //Debug.Log("TakePhyDam");

        if (!isServer)      // Ausschließlich der Server verursacht Schaden.
        {
            return;
        }

        CurrentHealth -= CalcTakenPhysicalDamage(pDamage);
    }

    protected float CalcTakenPhysicalDamage(float incomingDamage)
    {
        float damage = incomingDamage;
        damage -= armor.GetValue();
        damage = Mathf.Max(damage, 0);
        return damage;
    }
    #endregion

    #region Magical
    public void TakeMagicDamage(float mDamage)
    {
        if (!isServer)      // Ausschließlich der Server verursacht so Schaden.
        {
            return;
        }

        CurrentHealth -= CalcTakenPhysicalDamage(mDamage);
    }

    private float CalcTakenMagicalDamage(float incomingDamage)
    {
        float damage = incomingDamage;
        damage -= magicResistance.GetValue();
        damage = Mathf.Max(damage, 0);
        return damage;
    }

    #endregion

    public virtual void Die()
    {
        Debug.Log(transform.name + " died.");
    }

    #region Editor
    /// <summary>
    /// Draw AttackRange in Editor
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (attackRange.GetValue() == 0) return;

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, (float)attackRange.GetValue());
    }

    #endregion
}
