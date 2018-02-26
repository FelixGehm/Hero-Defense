using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(PlayerStats))]
public class NetworkCharacterStats : NetworkBehaviour
{
    public PlayerStats myStats;


  
    [SyncVar(hook = "OnChangedHealth")]
    public float syncedCurrentHealth;           

    private void OnChangedHealth(float currentHealth)
    {
        Debug.Log("OnChangedHealth: new Value = " + currentHealth);

        myStats.CurrentHealth = currentHealth;
    }
  

    void Awake()
    {
        myStats = GetComponent<PlayerStats>();

        syncedCurrentHealth = myStats.maxHealth.GetValue();
        myStats.isControlledByServer = true;
    }

    public void TakeDamage(float damage)
    {
        //Debug.Log("TakeDamage: dmg: "+ damage + " isServer = " + isServer );
        if (isServer)
        {
            //Debug.Log("Calced Damage: " + myStats.CalcTakenDamage(damage));
            syncedCurrentHealth -=  myStats.CalcTakenDamage(damage);
        }        
    }

    // Zum Testen
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
    }
}
