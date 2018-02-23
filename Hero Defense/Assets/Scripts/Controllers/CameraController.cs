using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    private Transform lookTarget;
    private Transform freeLookReference;
    
    public Vector3 offset;
    public float zoomSpeed = 4;
    public float minZoom = 5;
    public float maxZoom = 15;

    public float pitch = 2;

    public float panSpeed = 30;
    public float panBorderThickness = 10;

    private float currentZoom = 10;

    private bool centerCam = true;

    void Start()
    {
        
    }



    void Update()
    {
        currentZoom -= Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
        currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);

        if (Input.GetKeyDown("h")) ToggleCam();
    }

    void LateUpdate()
    {
        if (lookTarget != null)
        {
            if (centerCam)
            {
                FollowCam();
            }
            else
            {
                FreeCam();
            }
        }
        

    }

    public void SetLookAt(Transform target)
    {
        lookTarget = target;
        freeLookReference = new GameObject().transform;
        freeLookReference.position = lookTarget.position;
        //Camera.main = GetComponent<Camera>();
        
        FollowCam();
    }
    
    private void FollowCam()
    {
        transform.position = lookTarget.position - offset * currentZoom;
        transform.LookAt(lookTarget.position + Vector3.up * pitch);
    }

    private void FreeCam()
    {
        //Cam Up
        if(Input.mousePosition.y >= Screen.height - panBorderThickness)
            freeLookReference.Translate(Vector3.forward * panSpeed * Time.deltaTime, Space.World);

        //Cam Down
        if (Input.mousePosition.y <= panBorderThickness)
            freeLookReference.Translate(Vector3.back * panSpeed * Time.deltaTime, Space.World);

        //Cam Left
        if (Input.mousePosition.x <= panBorderThickness)
            freeLookReference.Translate(Vector3.left * panSpeed * Time.deltaTime, Space.World);

        //Cam Right
        if (Input.mousePosition.x >= Screen.width - panBorderThickness)
            freeLookReference.Translate(Vector3.right * panSpeed * Time.deltaTime, Space.World);

        transform.position = freeLookReference.position - offset * currentZoom;
        transform.LookAt(freeLookReference.position + Vector3.up * pitch);
    }

    public void ToggleCam()
    {
        centerCam = !centerCam;
        freeLookReference.position = lookTarget.position;
    }
}
