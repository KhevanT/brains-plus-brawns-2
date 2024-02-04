using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerController : MonoBehaviour
{
    // Movement variables
    public Vector2 startingPos = Vector2.zero;
    public float moveSpeed = 5f;
    public Transform movePoint; // helps keep track of player's next position

    // Collision variables
    public LayerMask obstaclesLayer;

    // Animation variables (may not need)
    public Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        // Set object to starting position
        transform.position = startingPos;

        // Make move point unparented to player object to ensure correct movement
        // In the scene its parented for better organisation
        movePoint.parent = null;
    }

    // Update is called once per frame
    void Update()
    {
        // Save current user input value
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        // Update player position based on movepoint pos
        transform.position = Vector3.MoveTowards(transform.position, movePoint.position, moveSpeed * Time.deltaTime);

        // Updates movepoint pos based on user input,
        // only if player is actually close to movepoint position first
        if (Vector3.Distance(transform.position, movePoint.position) <= 0.05f)
        {
            if (Mathf.Abs(horizontalInput) == 1f) // Update horizontal (X-axis) position
            {
                Move(new Vector3(horizontalInput, 0, 0));
            }
            else if (Mathf.Abs(verticalInput) == 1f) // Update vertical (Y-axis) position 
            {
                Move(new Vector3(0, verticalInput, 0));
            }
            // If-else if to prevent diagonal movement

            // Set moving to true if player is at desired position
            // animator.SetBool("isMoving", false);
        }
        else
        {
            // Set moving to true if player is not at desired position
            // animator.SetBool("isMoving", true);
        }
    }

    // Checks for collisions, updates position
    private void Move(Vector3 direction)
    {
        Vector3 newPosition = movePoint.position + direction;
        movePoint.position = newPosition;

        /*
        // Checks for potential overlap of new position with objects on obstacle layer
        if (Physics2D.OverlapCircle(newPosition, 0.2f, obstaclesLayer) == null)
        {
            movePoint.position = newPosition;
        }
        */
    }
}
