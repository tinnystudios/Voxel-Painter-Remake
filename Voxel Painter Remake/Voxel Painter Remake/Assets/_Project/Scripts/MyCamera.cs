using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyCamera : MonoBehaviour {

    public float horizontalSpeed = 10f; // looks like "10" maps to the native speed
    public float verticalSpeed = 10f;
    public Vector3 target;
    public Transform targetTransform;
    public float focalRange = 25;
    
    // Use this for initialization
    void Start () {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Focus(new Vector3(10,-1,10));
    }

    public void Focus(Vector3 v) {

        Vector3 dir = v - transform.position;
        dir.Normalize();
        transform.position = v;
        transform.position -= dir * focalRange;

        transform.LookAt(v);
        target = v;
    }

    // Update is called once per frame
    void Update ()
    {


        target = targetTransform.position;

        float scrollValue = Input.GetAxis("Mouse ScrollWheel");
        Vector3 newPos = transform.position + (transform.forward * scrollValue * 5000 * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, newPos, 20 * Time.deltaTime);

        float h = horizontalSpeed * Input.GetAxis("Mouse X");
        float v = verticalSpeed * Input.GetAxis("Mouse Y");

        Vector3 delta = new Vector3(h, v, 0);

        if (Input.GetKey(KeyCode.LeftAlt) && Input.GetMouseButton(0))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            transform.RotateAround(target, -transform.right, v * Time.deltaTime);
            transform.RotateAround(target, transform.up, h * Time.deltaTime);
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        Vector3 nAng = transform.eulerAngles;
        nAng.z = 0;
        transform.eulerAngles = nAng;

        if (Input.GetMouseButton(2)) {
            transform.localPosition += -transform.up * delta.y * Time.deltaTime;
            transform.localPosition += -transform.right * delta.x * Time.deltaTime;
            target = transform.position + (transform.forward * focalRange);
        }


        if (Input.GetKeyDown(KeyCode.F))
            Focus(target);

    }
}

