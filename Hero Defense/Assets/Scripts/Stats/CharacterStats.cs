using UnityEngine;
using UnityEngine.Networking;


/// <summary>
/// Muss von NetworkBehaviour erben, da auf diverse Variablen daraus zugegriffen wird, und mit SyncVars gearbeitet wird.
/// </summary>
public class CharacterStats : NetworkBehaviour
{
    public virtual void Awake()
    {
            syncedCurrentHealth = maxHealth.GetValue();
    }

    protected static float timeBetweenTicksForHealthRegeneration = 1.0f;
    protected float currentTickCoolDown = timeBetweenTicksForHealthRegeneration;
    
    protected void Update()
    {
        if (!isServer)
        {
            return;
        }

        currentTickCoolDown -= Time.deltaTime;

        if (currentTickCoolDown <= 0)
        {
            currentTickCoolDown = timeBetweenTicksForHealthRegeneration;


            if (syncedCurrentHealth > 0)
            {
                syncedCurrentHealth += maxHealth.GetValue() * healthRegeneration.GetValue();
            }
        }
    }

    [SyncVar(hook ="OnSyncedCurrendHealthChanged")]
    private float syncedCurrentHealth;

    public float SyncedCurrentHealth
    {
        set
        {
            syncedCurrentHealth = value;

            if (syncedCurrentHealth <= 0)
            {
                Die();
            }
        }
        get
        {
            return syncedCurrentHealth;
        }                 
    }

    void OnSyncedCurrendHealthChanged(float newHealth)
    {
        SyncedCurrentHealth = newHealth;
    }

    public bool IsAlive()
    {                    
            return syncedCurrentHealth >= 0;
    }

    public Stat maxHealth;


    [Tooltip("0.0 = 0% Health returned per tick, 1.0 = 100% Health returned per tick")]
    public Stat healthRegeneration;

    public Stat physicalDamage;
    public Stat armor;

    public Stat magicDamage;
    public Stat magicResistance;

    [Tooltip("0.0 = 0% CritChance, 1.0 = 100% CritChance")]
    public Stat critChance;

    [Tooltip("1.0 = 100% CritDamage, 1.5 = 150% CritDamage, 2 = Crit200% Damage")]
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

        SyncedCurrentHealth -= damage;
    }

    #endregion

    #region Heal
    public void TakeHeal(float healAmount)
    {
        if (!isServer) return;

        SyncedCurrentHealth += healAmount;
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

        SyncedCurrentHealth -= CalcTakenPhysicalDamage(pDamage);
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

        SyncedCurrentHealth -= CalcTakenPhysicalDamage(mDamage);
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
