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
        if(isServer)
        {
           syncedCurrentHealth -=  myStats.CalcTakenDamage(damage);
        }        
    }

    private void Update()
    {
        if(Input.GetKeyDown("d"))
        {
            Debug.Log("TAKE DAMAGE");
            TakeDamage(10.0f);
        }
    }
}
