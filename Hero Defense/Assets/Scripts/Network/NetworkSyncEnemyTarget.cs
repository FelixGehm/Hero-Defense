using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

[RequireComponent(typeof(EnemyController))]
public class NetworkSyncEnemyTarget : NetworkBehaviour
{

    /*
     * AKTUELL NICHT FUNKTIONSFÄHIG UND WIRD AUCH NIRGENS VERWENDET
     * 
     * */

    private EnemyController enemyController;

    void Awake()
    {
        enemyController = GetComponent<EnemyController>();

        //enemyController.onTargetChanged += SetTargetForAllInstances;

    }

    /*
     * Problem: EnemyController wird ausgeschaltet bei nicht Servern -> ontargetCHanged wird nie aufgerufen von Clients
     * 
     * */
    public void SetTargetForAllInstances(Transform target)
    {
        
        // Projektil vom Server erzeugen lassen bzw. als Server selbst das Projektil für alle spawnen
        if (isServer)
        {
            syncedCurrentTargetId = target.GetComponent<NetworkInstanceId>();
        }
        else
        {
            TransmitTarget(target.GetComponent<NetworkInstanceId>());            
        }
    }



    [SyncVar(hook = "OnChangedTarget")]
    public NetworkInstanceId syncedCurrentTargetId;

    private void OnChangedTarget(NetworkInstanceId targetId)
    {
        //Debug.Log("OnChangedHealth: new Value = " + currentHealth);
       // enemyController.Target = NetworkServer.FindLocalObject(targetId).transform;
    }
    

    [Command]
    void CmdProvideTargetToServer(NetworkInstanceId id)
    {
        syncedCurrentTargetId = id;
    }

    [ClientCallback]
    void TransmitTarget(NetworkInstanceId id)
    {
        {
            CmdProvideTargetToServer(id);
        }
    }

}
