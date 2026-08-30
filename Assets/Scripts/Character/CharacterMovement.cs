using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Animator animator;
    [SerializeField] private LimbIKController limbController;
    
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float jumpForce = 5f;
    
    private bool isGrounded = true;
    private string currentState = "idle";
    
    private Vector2 p1Input, p2Input;
    private bool jumpPressed;
    
    private void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (animator == null) animator = GetComponent<Animator>();
        if (limbController == null) limbController = GetComponent<LimbIKController>();
    }
    
    // Called from NetworkPlayer every frame
    public void UpdateMovement(Vector2 player1Input, Vector2 player2Input, bool jump)
    {
        p1Input = player1Input;
        p2Input = player2Input;
        jumpPressed = jump;
        
        // Drive limb IK with each player's individual input
        if (limbController != null)
            limbController.UpdateLimbs(p1Input, p2Input);
    }
    
    private void FixedUpdate()
    {
        // Combined movement: both players must push for character to move
        Vector2 combined = p1Input + p2Input;
        Vector3 moveDirection = new Vector3(combined.x, 0, combined.y);
        float inputMagnitude = moveDirection.magnitude;
        
        if (inputMagnitude > 0.05f)
        {
            Vector3 dir = moveDirection.normalized;
            rb.linearVelocity = new Vector3(dir.x * moveSpeed, rb.linearVelocity.y, dir.z * moveSpeed);
            
            Quaternion targetRotation = Quaternion.LookRotation(dir);
            rb.rotation = Quaternion.Lerp(rb.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
        else
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }
        
        // Jump
        if (jumpPressed && isGrounded)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
            animator.SetBool("param_idletojump", true);
            currentState = "jump";
            isGrounded = false;
        }
        else
        {
            animator.SetBool("param_idletojump", false);
        }
        
        // Animation state
        if (inputMagnitude < 0.1f)
        {
            animator.SetBool("param_idletowalk", false);
            animator.SetBool("param_idletorunning", false);
            currentState = "idle";
        }
        else if (inputMagnitude > 1.5f)
        {
            animator.SetBool("param_idletowalk", false);
            animator.SetBool("param_idletorunning", true);
            currentState = "running";
        }
        else
        {
            animator.SetBool("param_idletorunning", false);
            animator.SetBool("param_idletowalk", true);
            currentState = "walk";
        }
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground")) isGrounded = true;
    }
    
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground")) isGrounded = false;
    }
}