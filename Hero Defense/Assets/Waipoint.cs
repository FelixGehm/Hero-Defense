using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waipoint : MonoBehaviour {

    public float radius = 2.0f;

    private void Update()
    {
        
        if(Input.GetKeyDown(KeyCode.P))
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);

            obj.transform.position = GetDestinationInRadius();
        }
        
    }

    public Vector3 GetDestinationInRadius()
    {
        Vector3 destination = this.transform.position;

        float rAngle = Random.Range(0, 360);
        float rRadius = Random.Range(0, radius);

        Vector3 vecToAdd = Quaternion.Euler(0, rAngle,0) * (Vector3.forward * rRadius);
        return destination + vecToAdd;
    }


    private void OnDrawGizmosSelected()
    {
        UnityEditor.Handles.color = Color.red;
        UnityEditor.Handles.DrawWireDisc(transform.position, transform.up, radius);
    }
}
