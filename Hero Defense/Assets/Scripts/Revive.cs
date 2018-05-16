using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


//Gives a player the ability to revive another player
public class Revive : AbilityBasic
{

    private bool wlnd;



    // Use this for initialization
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        if (Input.GetMouseButtonDown(0))
        {
            Cast();
        }
    }

    [Client]
    protected override void Cast()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100))
        {
            if (hit.collider.tag == "Player")
            {
                Debug.Log(hit.collider.name + "was hit");
                CmdRevivePlayer(hit.collider.name);
            }
        }
    }

    [Command]
    void CmdRevivePlayer(string _ID)
    {
        Debug.Log(_ID + " has been revived.");
        GameObject.Find(_ID).GetComponent<PlayerController>().RevivePlayer();
    }
}
