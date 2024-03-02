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

    public Camera mainCam;

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


        if (Input.GetMouseButtonDown(0))  {//hitting, breaking // Left click
            // Cast a ray from the screen point where the mouse is pointing
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Check if the ray hits an object
            if (Physics.Raycast(ray, out hit)) {
                MeshFilter meshFilter = hit.collider.GetComponent<MeshFilter>();
                if (meshFilter != null) {
                    // Get the mesh and its vertices
                    Mesh mesh = meshFilter.mesh;
                    Vector3[] vertices = mesh.vertices;

                    // Find the closest vertex to the hit point
                    int closestVertexIndex = FindClosestVertexIndex(vertices, hit.point);

                    // Move the closest vertex based on mouse position
                    float newY = mainCam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, hit.distance)).y;
                    vertices[closestVertexIndex].y = newY;
                    AdjustNeighboringVertices(vertices, closestVertexIndex, newY);


                    // Update the mesh with the new vertices
                    mesh.vertices = vertices;
                    mesh.RecalculateNormals();
                }
            }
        }

        if (Input.GetMouseButtonDown(1)){ //right click grab, interact
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Check if the ray hits an object
            if (Physics.Raycast(ray, out hit)){

            }

        }
    }

    int FindClosestVertexIndex(Vector3[] vertices, Vector3 point) {
        int closestIndex = 0;
        float closestDistance = Vector3.Distance(vertices[0], point);

        for (int i = 1; i < vertices.Length; i++) {
            float distance = Vector3.Distance(vertices[i], point);
            if (distance < closestDistance){
                closestIndex = i;
                closestDistance = distance;
            }
        }

        return closestIndex;
    }

    void AdjustNeighboringVertices(Vector3[] vertices, int centralIndex, float newY) {
        for (int i = 0; i < vertices.Length; i++) {
            if (i != centralIndex) {
                float distance = Vector3.Distance(vertices[i], vertices[centralIndex]);
                float proportion = 1.0f - Mathf.Clamp01(distance / 2.0f); // Adjust the divisor for the influence radius
                vertices[i].y += (newY - vertices[centralIndex].y) * proportion;
            }
        }
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
