using UnityEngine;
using UnityEngine.SceneManagement;

public class WaveManager : MonoBehaviour
{
    public PlayerManager playerManager;

    public WaveSpawnerSlave topSpawner;
    public WaveSpawnerSlave midSpawner;
    public WaveSpawnerSlave botSpawner;


    public Wave[] allWaves;
    public Wave currentWave;
    public int currentWaveNo = 1;

    




    void Start()
    {
        if (topSpawner == null)
        {
            Debug.LogWarning("No topSpawner attached to WaveManager!");
        }

        if (midSpawner == null)
        {
            Debug.LogWarning("No midSpawner attached to WaveManager!");
        }

        if (botSpawner == null)
        {
            Debug.LogWarning("No botSpawner attached to WaveManager!");
        }

        LoadAllWaves();


    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown("s"))
        {
            TellSlavesToStartWave(currentWave);

            currentWave = GetNextWave();
        }
    }

    private void LoadAllWaves()
    {
        allWaves = Resources.LoadAll<Wave>("Waves");

        currentWave = allWaves[0];
    }

    private Wave GetNextWave()
    {
        // TODO: Sicher machen für Letzte Wave erreicht
        //if(currentWaveNo < allWaves.Length)
        currentWaveNo++;
        Wave tmp = allWaves[currentWaveNo - 1];

        return tmp;
    }

    private void TellSlavesToStartWave(Wave wave)
    {
        int currentPlayers = playerManager.GetNoOfPlayers();

        
        int melee = currentWave.noOfMelee * currentPlayers;
        int ranged = currentWave.noOfRanged * currentPlayers;
        int sRanged = currentWave.noOfSpecialRanged *currentPlayers;
        int boss = currentWave.noOfBosses;  //TODO Boss verstärken anstelle von Anzahl erhöhen

        float timeSpan = ((float)(melee/3 + ranged/3 + sRanged/3 + boss)) / 0.5f;
        Debug.Log("timeSpan = " + timeSpan);

        topSpawner.Spawn(melee/3, ranged/3, sRanged/3, 0, timeSpan);
        midSpawner.Spawn(melee / 3, ranged / 3, sRanged / 3, 0, timeSpan);
        botSpawner.Spawn(melee / 3, ranged / 3, sRanged / 3, boss, timeSpan);
    }
}
