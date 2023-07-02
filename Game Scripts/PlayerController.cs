using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour

{

    // Public variables for speed, jump height and for checking if the game is over or paused.
    public float speed;

    // Private floats that don't need to be accessed, by how much the character "crouches", the horizontal input for moving left and right
    // and whether the player is on the ground or is crouching.
    private float horizontalInput;
    private float verticalInput;
    
    private Rigidbody playerRb;

    private void Start()

    {

        // Gets the components and the current scale of the player.
        playerRb = GetComponent<Rigidbody>();

    }
    
    private void FixedUpdate()

    {

        // Gets the input in the fixed update to make it smoother.
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
        MovementCheck();

    }

    public void MovementCheck()

    {

        // Move the player left and right depending on the horizontal input gotten by holding A or D.
        transform.Translate(Vector3.right * (speed * horizontalInput * Time.deltaTime));
        transform.Translate(Vector3.forward * (speed * verticalInput * Time.deltaTime));

    }

}