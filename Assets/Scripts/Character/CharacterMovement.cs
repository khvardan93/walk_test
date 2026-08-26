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
    }
    
    private void FixedUpdate()
    {
        if (lastInput.movement == Vector2.zero && !lastInput.jumpPressed)
            return;
        
        // MOVEMENT: Combined direction from both players
        Vector3 moveDirection = new Vector3(lastInput.movement.x, 0, lastInput.movement.y);
        float inputMagnitude = moveDirection.magnitude;
        
        // Apply movement using AddForce (works better than setting velocity)
        Vector3 force = moveDirection.normalized * moveSpeed;
        rb.linearVelocity = new Vector3(force.x, rb.linearVelocity.y, force.z);
        
        Debug.Log("Movement: " + moveDirection + ", Magnitude: " + inputMagnitude);
        
        // ROTATION
        if (inputMagnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            rb.rotation = Quaternion.Lerp(rb.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
        
        // JUMP
        if (lastInput.jumpPressed && isGrounded)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
            animator.SetBool("param_Idletojump", true);
            currentState = "jump";
            isGrounded = false;
        }
        else
        {
            animator.SetBool("param_Idletojump", false);
        }
        
        // ANIMATIONS based on speed
        if (inputMagnitude < 0.1f)
        {
            animator.SetBool("param_Idletowalk", false);
            animator.SetBool("param_Idletorunning", false);
            currentState = "idle";
        }
        else if (inputMagnitude > 0.5f)
        {
            animator.SetBool("param_Idletowalk", false);
            animator.SetBool("param_Idletorunning", true);
            currentState = "running";
        }
        else
        {
            animator.SetBool("param_Idletorunning", false);
            animator.SetBool("param_Idletowalk", true);
            currentState = "walk";
        }
    }
    
    private void OnCollisionEnter(Collision collision)
    {
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