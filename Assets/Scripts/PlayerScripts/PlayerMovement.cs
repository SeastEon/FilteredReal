using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

    [Header("Movement")]
    public float movementspeed;

    public float groundDrag;

    public float jumpForce;
    public float JumpCoolDown;
    public float airMultiplier;
    bool readyToJump;

    [Header("KeyBinds")]
    public KeyCode jumpKey = KeyCode.Space;

    [Header("Ground Check")]
    public float playerheight;
    public LayerMask WhatIsGround;
    bool grounded;

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 Movedirection;

    Rigidbody rb;

    private void Start() {
        readyToJump = true;
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    private void Update() {
        //ground  Check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerheight * 0.5f +0.2f, WhatIsGround);
        //Input update
        MyInput();
        SpeedControl();

        if (grounded){rb.drag = groundDrag;} 
        else { rb.drag = 0; }
    }

    private void FixedUpdate() { MovePlayer(); }

    private void MyInput() {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        //jump 
        if (Input.GetKey(jumpKey) && readyToJump && grounded) {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), JumpCoolDown);
        }
    }

    private void MovePlayer() {
    //calculate movement;
        Movedirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        if (grounded) { rb.AddForce(Movedirection.normalized * movementspeed * 10f, ForceMode.Force); }
        else if(!grounded){ rb.AddForce(Movedirection.normalized *movementspeed * airMultiplier * 10f, ForceMode.Force); }

    }

    private void SpeedControl() {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.y);

        if(flatVel.magnitude > movementspeed) {
            Vector3 limitedVel = flatVel.normalized * movementspeed;
            rb.velocity= new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

    private void Jump() {
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump() { readyToJump = true; }
}
