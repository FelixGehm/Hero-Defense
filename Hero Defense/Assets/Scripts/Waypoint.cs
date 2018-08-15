using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waypoint : MonoBehaviour
{

    public float radius = 2.0f;

    [HideInInspector]
    public Waypoint next;

    public bool isNexus = false;
    private Waypoint nexus;

    private void Start()
    {
        if (!isNexus)
        {
            nexus = transform.parent.parent.GetComponent<Waypoint>();
            next = SetupNext();
        }
        else
        {
            nexus = this;
            next = this;
        }
    }

    private Waypoint SetupNext()
    {
        if (next == nexus)
        {
            return nexus;
        }

        int noOfWaypoints = transform.parent.childCount;

        int ownIndex = 0;

        for (int i = 0; i < noOfWaypoints; i++)
        {
            if (this.gameObject.Equals(transform.parent.GetChild(i).gameObject))
            {
                ownIndex = i;
                break;
            }
        }

        Waypoint waypoint;
        if (ownIndex + 1 < noOfWaypoints)
        {
            waypoint = transform.parent.GetChild(ownIndex + 1).GetComponent<Waypoint>();
        }
        else
        {
            waypoint = nexus;
        }


        return waypoint;
    }


    private void Update()
    {
        /*
        if(Input.GetKeyDown(KeyCode.P))
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);

            obj.transform.position = GetDestination();
        }
        */
    }

    public Vector3 GetDestinationInRadius()
    {
        Vector3 destination = this.transform.position;

        float rAngle = Random.Range(0, 360);
        float rRadius = Random.Range(0, radius);

        Vector3 vecToAdd = Quaternion.Euler(0, rAngle, 0) * (Vector3.forward * rRadius);
        return destination + vecToAdd;
    }


    private void OnDrawGizmosSelected()
    {
        //UnityEditor.Handles.color = Color.red;
        //UnityEditor.Handles.DrawWireDisc(transform.position, transform.up, radius);
    }
}
