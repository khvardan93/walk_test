using UnityEngine;

public class StepController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Animator animator;

    [Header("Animator")]
    [SerializeField] private string walkStateName = "walk";
    [SerializeField] private string idleStateName = "idle";

    [Header("Step Tuning")]
    [SerializeField] private float inputThreshold = 0.5f;
    [SerializeField] private float stepDuration = 0.35f;
    [SerializeField] private float strideDistance = 0.6f;
    [SerializeField] private float turnStepAngle = 45f;   // degrees rotated per turn-step
    [SerializeField] private float idleResetTime = 2f;

    // Whose turn it is. Player 1 owns the left leg and goes first.
    private bool leftLegsTurn = true;

    // Manual playback position through the walk cycle, 0..1
    private float gaitTime = 0f;

    // Active step sweep
    private bool stepping = false;
    private float stepElapsed = 0f;
    private float gaitStart = 0f;
    private float gaitEnd = 0f;

    // What the active step does: advance forward, rotate, or both.
    // A walk-step sets forward; a turn-step sets turn. Both scrub the walk clip.
    private float activeStepForward = 0f; // meters
    private float activeStepTurn = 0f;    // degrees

    // Rising edge detection (forward = y, turn = x) per player
    private float p1PrevForward = 0f;
    private float p2PrevForward = 0f;
    private float p1PrevTurn = 0f;
    private float p2PrevTurn = 0f;

    private float timeSinceLastStep = 0f;

    // Stay in the controller's default idle until the very first step
    private bool everStepped = false;

    private void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (animator == null) animator = GetComponent<Animator>();
    }

    public void Tick(Vector2 p1Input, Vector2 p2Input)
    {
        HandleStepInput(p1Input, p2Input);
        AdvanceStep();
        DriveAnimator();
    }

    private void HandleStepInput(Vector2 p1Input, Vector2 p2Input)
    {
        float p1Forward = p1Input.y, p2Forward = p2Input.y;
        float p1Turn = p1Input.x, p2Turn = p2Input.x;

        if (!stepping)
        {
            // Rising edge: was below threshold, now at/above. Forward taps a
            // walk-step; a sideways tap turns. Only the player whose turn it is
            // may act, and forward wins if both fire on the same frame.
            bool fwdFired = RisingEdge(leftLegsTurn ? p1PrevForward : p2PrevForward,
                                       leftLegsTurn ? p1Forward : p2Forward);
            float turnNow = leftLegsTurn ? p1Turn : p2Turn;
            bool turnFired = RisingEdge(Mathf.Abs(leftLegsTurn ? p1PrevTurn : p2PrevTurn),
                                        Mathf.Abs(turnNow));

            if (fwdFired) BeginStep(strideDistance, 0f);
            else if (turnFired) BeginStep(0f, Mathf.Sign(turnNow) * turnStepAngle);
        }

        p1PrevForward = p1Forward; p2PrevForward = p2Forward;
        p1PrevTurn = p1Turn; p2PrevTurn = p2Turn;
    }

    private static bool RisingEdge(float prev, float now)
    {
        return prev < 0.5f && now >= 0.5f;
    }

    private void BeginStep(float forward, float turn)
    {
        stepping = true;
        everStepped = true;
        stepElapsed = 0f;
        gaitStart = gaitTime;
        gaitEnd = gaitStart + 0.5f; // half a cycle = one step
        timeSinceLastStep = 0f;
        activeStepForward = forward;
        activeStepTurn = turn;
    }

    private void AdvanceStep()
    {
        if (!stepping)
        {
            timeSinceLastStep += Time.deltaTime;

            // Settle back to a neutral turn order if both players stop
            if (timeSinceLastStep > idleResetTime)
            {
                leftLegsTurn = true;
            }
            return;
        }

        stepElapsed += Time.deltaTime;
        float t = Mathf.Clamp01(stepElapsed / stepDuration);

        // Scrub the animation across half a gait cycle
        gaitTime = Mathf.Lerp(gaitStart, gaitEnd, t);

        // Move / rotate in lockstep with the animation
        float frac = Time.deltaTime / stepDuration;
        if (activeStepForward != 0f)
            rb.MovePosition(rb.position + transform.forward * (activeStepForward * frac));
        if (activeStepTurn != 0f)
            transform.Rotate(0f, activeStepTurn * frac, 0f);

        if (t >= 1f)
        {
            stepping = false;
            gaitTime = gaitEnd % 1f;      // wrap cleanly
            leftLegsTurn = !leftLegsTurn; // pass the turn to the other player
        }
    }

    private void DriveAnimator()
    {
        // Drive the walk clip purely by manual scrub. We deliberately do NOT
        // touch the bool transition parameters: letting the walk state run on
        // its own would auto-advance and loop once we stop scrubbing, instead
        // of holding one discrete step. By never setting a bool, none of idle's
        // transition conditions fire, so the state stays exactly where
        // Animator.Play puts it.
        //
        // Pinning normalizedTime to gaitTime every frame means: while stepping,
        // gaitTime advances so a half gait-cycle plays; between steps gaitTime
        // is constant so the pose freezes on the last stride.
        if (!stepping && !everStepped)
            return; // untouched -> character shows the controller's default idle

        animator.Play(walkStateName, 0, gaitTime % 1f);
        animator.Update(0f);
    }

    // Exposed so UI or debugging can show whose turn it is
    public bool IsLeftLegsTurn => leftLegsTurn;
    public bool IsStepping => stepping;
}
