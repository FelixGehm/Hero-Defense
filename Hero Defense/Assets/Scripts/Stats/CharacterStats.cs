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

    [SyncVar]
    private float syncedCurrentHealth;
    public virtual float CurrentHealth
    {
        get
        {
            return syncedCurrentHealth;
        }
        set
        {
            Debug.Log("Health Changed to " + value);
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

    public Stat maxHealth;

    public Stat physicalDamage;
    public Stat armor;

    public Stat magicDamage;
    public Stat magicResistance;

    public Stat critChance;
    public Stat critDamage;

    public Stat attackSpeed;
    public Stat attackRange;

    public Stat moveSpeed;

    // TODO: Stats im Netzwerk synchronisieren

    #region Physical
    public void TakePhysicalDamage(float pDamage)
    {
        Debug.Log("TakePhyDam");

        if (!isServer)      // Ausschließlich der Server verursacht so Schaden.
        {
            return;
        }

        //if (!isControlledByServer)
        {
            CurrentHealth -= CalcTakenPhysicalDamage(pDamage);

            Debug.Log(transform.name + " takes " + pDamage + " pDamage");

            if (CurrentHealth <= 0)
                Die();
        }
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

        //if (!isControlledByServer)
        {
            CurrentHealth -= CalcTakenPhysicalDamage(mDamage);

            Debug.Log(transform.name + " takes " + mDamage + " mDamage");

            if (CurrentHealth <= 0)
                Die();
        }
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

    #region Network
    /// <summary>
    /// Für eine (relativ) ausführliche Erklärung zu Command und ClientCallBack:
    ///     siehe CharacterEventManager
    /// </summary>

    /*
    [Command]
    void CmdProvideHealthToServer(float health)
    {
        Debug.Log("CmdProvideHealthToServer:");
        syncedCurrentHealth = health;
    }

    [ClientCallback]
    void TransmitAttack()
    {
        //Debug.Log(transform.name + " TransmitAttack(): isServer = " + isServer + " hasAuthority = " + hasAuthority);
        {
            CmdProvideHealthToServer(syncedCurrentHealth);
        }
    }
    */
    #endregion


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
