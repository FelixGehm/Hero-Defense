using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(CharacterStats))]
public class NetworkCharacterStats : NetworkBehaviour
{
    private CharacterStats myStats;


  
    [SyncVar(hook = "OnChangedHealth")]
    public float syncedCurrentHealth;           

    private void OnChangedHealth(float currentHealth)
    {
        //Debug.Log("OnChangedHealth: new Value = " + currentHealth);

        myStats.CurrentHealth = currentHealth;
    }
  

    void Awake()
    {
        myStats = GetComponent<CharacterStats>();

        syncedCurrentHealth = myStats.maxHealth.GetValue();
        myStats.isControlledByServer = true;
    }

    
    public void TakeDamage(float damage)
    {
        //Debug.Log("TakeDamage: dmg: "+ damage + " isServer = " + isServer + "; isLocalPlayer = "+isLocalPlayer);
        //if (isServer)
        {
            //Debug.Log("Calculated Damage: " + myStats.CalcTakenDamage(damage));
            syncedCurrentHealth -=  myStats.CalcTakenDamage(damage);

            //TransmitAttack();
        }        
    }

    public CharacterStats getStats()
    {
        return myStats;
    }

    [Command]
    void CmdProvideHealthToServer(float health)
    {
        Debug.Log("CmdProvideHealthToServer:");
        syncedCurrentHealth = health;
    }

    [ClientCallback]
    void TransmitAttack()
    {
        //Debug.Log(transform.name + " TransmitAttack(): isLocalPlayer = " + isLocalPlayer);
        Debug.Log(transform.name + " TransmitAttack(): isServer = " + isServer + " hasAuthority = " + hasAuthority);
        //if (isLocalPlayer)
        //if (!isServer)
        {
            CmdProvideHealthToServer(syncedCurrentHealth);
        }
    }

    /* Zum Testen
    private void Update()
    {
        if(Input.GetKeyDown("d"))
        {
            if (!isLocalPlayer)
            {
                TakeDamage(10.0f);
            }
        }

        if (Input.GetKeyDown("a"))
        {
            if (isLocalPlayer)
            {
                TakeDamage(10.0f);
            }

        }
    }*/
}
