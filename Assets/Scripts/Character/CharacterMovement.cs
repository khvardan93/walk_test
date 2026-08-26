using Fusion;
using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Animator animator;
    
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float jumpForce = 5f;
    
    private NetworkInputData lastInput;
    private bool isGrounded = true;
    private string currentState = "idle";
    
    private void Start()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();
        
        if (animator == null)
            animator = GetComponent<Animator>();
    }
    
    public void UpdateMovement(NetworkInputData input)
    {
        lastInput = input;
        
        // MOVEMENT: Combined direction from both players
        Vector3 moveDirection = new Vector3(input.movement.x, 0, input.movement.y);
        
        // Calculate speed
        float inputMagnitude = moveDirection.magnitude;
        
        // Apply movement to Rigidbody
        Vector3 velocity = rb.linearVelocity;
        velocity.x = moveDirection.x * moveSpeed;
        velocity.z = moveDirection.y * moveSpeed;
        rb.linearVelocity = velocity;
        
        // ROTATION: Rotate character to face movement direction
        if (inputMagnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            rb.rotation = Quaternion.Lerp(rb.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
        
        // ANIMATION STATE MACHINE
        if (input.jumpPressed && isGrounded)
        {
            // JUMP
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
            animator.SetTrigger("param_Idletojump");
            currentState = "jump";
            isGrounded = false;
        }
        else if (inputMagnitude < 0.1f)
        {
            // IDLE
            if (currentState != "idle")
            {
                // We're stopping - don't trigger anything, just let animation fade
                currentState = "idle";
            }
        }
        else if (inputMagnitude > 0.5f)
        {
            // RUNNING (high speed)
            if (currentState != "running")
            {
                animator.SetTrigger("param_Idletorunning");
                currentState = "running";
            }
        }
        else
        {
            // WALKING (low speed)
            if (currentState != "walk")
            {
                animator.SetTrigger("param_Idletowalk");
                currentState = "walk";
            }
        }
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        // Check if we hit the ground
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }
    
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }
}