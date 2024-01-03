using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamMovement : MonoBehaviour
{
    public float sensX;
    public float sensY;

    public Transform orientation;

    float XRotation;
    float YRotation;

    private void Start() {
        Cursor.lockState = CursorLockMode.Locked; //sets the cursor to the middle of the screen
        Cursor.visible = false;
    }

    private void Update()
    {
        float MouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
        float MouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

        YRotation += MouseX;
        XRotation -= MouseY;

        XRotation = Mathf.Clamp(XRotation, -90f, 90f);

        transform.rotation = Quaternion.Euler(XRotation, YRotation, 0);
        orientation.rotation = Quaternion.Euler(0, YRotation, 0);

        //float MouseX = Input.mousePosition.x;
        //float MouseY = Input.mousePosition.y;
    }

}
