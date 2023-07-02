using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseMovement : MonoBehaviour

{

    public float mouseSens = 100.0f;

    public Transform playerBody;
    
    private float xRotation = 0.0f;

    // Start is called before the first frame update
    void Start()

    {

        // lock the cursor
        Cursor.lockState = CursorLockMode.Locked;

    }

    // Update is called once per frame
    void Update()
    
    {
        
        // get input from mouse for x, multiply by sens
        float mouseX = Input.GetAxisRaw("Mouse X") * mouseSens * Time.fixedDeltaTime;
        // do same for y
        float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSens * Time.fixedDeltaTime;
        
        // invert y
        xRotation -= mouseY;

        // clamp camera
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // rotate using x rotation
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        // rotate the player body
        playerBody.Rotate(Vector3.up * mouseX);
        
    }
    
}