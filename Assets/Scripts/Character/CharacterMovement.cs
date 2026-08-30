using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Animator animator;
    [SerializeField] private LimbIKController limbController;
    [SerializeField] private StepController stepController;

    private Vector2 p1Input, p2Input;

    private void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (animator == null) animator = GetComponent<Animator>();
        if (limbController == null) limbController = GetComponent<LimbIKController>();
        if (stepController == null) stepController = GetComponent<StepController>();
    }

    public void UpdateMovement(Vector2 player1Input, Vector2 player2Input, bool jump)
    {
        p1Input = player1Input;
        p2Input = player2Input;

        // Arms still show the split control visually
        if (limbController != null)
            limbController.UpdateLimbs(p1Input, p2Input);

        // Legs and forward motion are entirely the step system's job
        if (stepController != null)
            stepController.Tick(p1Input, p2Input);
    }
}