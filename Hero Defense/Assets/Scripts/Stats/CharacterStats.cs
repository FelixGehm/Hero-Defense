using UnityEngine;
using UnityEngine.Networking;


/// <summary>
/// Muss von NetworkBehaviour erben, da auf diverse Variablen daraus zugegriffen wird, und mit SyncVars gearbeitet wird.
/// </summary>
public class CharacterStats : NetworkBehaviour
{


    public HealthBarManager healthBarManager;
    private UIHealthBar uIHealthBar;

    void Awake()
    {
        CurrentHealth = maxHealth.GetValue();

        uIHealthBar = GameObject.Find("Canvas HUD").transform.Find("CharacterInfo").Find("UIHealthBar").GetComponent<UIHealthBar>();

        //isLocalPlayer = string.Compare("LocalPlayer", gameObject.name.Substring(0, 10));
        isEnemy = gameObject.CompareTag("Enemy");
    }

    [HideInInspector]
    public bool isControlledByServer = false;

    private bool isEnemy;


    /// <summary>
    /// Bei SyncVars kann man sogenannte Hooks anhängen. Das sind Methoden, die ausgeführt werden, sobald der Server die aktuallisierte Variable gesendet hat. 
    /// Sobald der Server den Wert für eine SyncVar bei den Clients ändert, wird dieser Hook dann ausgeführt.
    /// In diesem Fall wird dies angewandt, umd den Property, der auf die Syncvar verweist ebenfalls zu aktuallisieren.
    /// </summary>
    [SyncVar(hook = "OnChangeHealth")]
    private float syncedCurrentHealth;

    public virtual float CurrentHealth
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
                }
            }
        }
    }


    private void OnChangeHealth(float newHealth)
    {
        CurrentHealth = newHealth;      //Property CurrentHealth muss ebenfalls synchronisiert werden, damit die Healthbar angepasst wird.
    }


    public Stat maxHealth;

    public Stat physicalDamage;
    public Stat armor;

    public Stat magicDamage;
    public Stat magicResistance;

    [Header("0.0 = 0% CritChance, 1.0 = 100% CritChance")]
    public Stat critChance;

    [Header ("1.0 = 100% Damage, 1.5 = 150% Damage, 2 = 200% Damage")]
    public Stat critDamage;     

    public Stat attackSpeed;
    public Stat attackRange;

    public Stat moveSpeed;

    // TODO: Stats im Netzwerk synchronisieren

    #region TrueDamage
    public void TakeTrueDamage(float tDamage)
    {
        //Debug.Log("TakePhyDam");

        if (!isServer)      // Ausschließlich der Server verursacht so Schaden.
        {
            return;
        }        
        
        float damage = tDamage;
        damage = Mathf.Max(damage, 0);      // Check if damage is < 0, if yes -> set to 0

        CurrentHealth -= damage;
    }

    #endregion

    #region Physical
    public void TakePhysicalDamage(float pDamage)
    {
        //Debug.Log("TakePhyDam");

        if (!isServer)      // Ausschließlich der Server verursacht so Schaden.
        {
            return;
        }

        CurrentHealth -= CalcTakenPhysicalDamage(pDamage);
    }

    private float CalcTakenPhysicalDamage(float incomingDamage)
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

    public float CalcTakenMagicalDamage(float incomingDamage)
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
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, (float)attackRange.GetValue());
    }

    #endregion
}
