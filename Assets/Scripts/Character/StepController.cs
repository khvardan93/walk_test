using UnityEngine;

public class StepController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Animator animator;

    [Header("Animator")]
    [SerializeField] private string walkStateName = "walk";
    [SerializeField] private string idleStateName = "idle";
    [SerializeField] private string jumpStateName = "jump";

    [Header("Step Tuning")]
    [SerializeField] private float inputThreshold = 0.5f;
    [SerializeField] private float stepDuration = 0.35f;
    [SerializeField] private float strideDistance = 0.6f;
    [SerializeField] private float turnStepAngle = 20f;   // degrees rotated per turn-step

    [Header("Jump Tuning")]
    [SerializeField] private float jumpSpeed = 6f;             // upward velocity of a full (both-player) jump
    [SerializeField] private float soloJumpFactor = 0.6f;     // fraction of the height when only one player jumps
    [SerializeField] private float soloJumpSideSpeed = 1.5f;  // sideways velocity nudging a solo jump toward the jumper
    [SerializeField] private float jumpBufferTime = 0.12f;    // window to catch the other player's press as "together"
    [SerializeField] private string groundTag = "Ground";

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

    // Stay in the controller's default idle until the very first step
    private bool everStepped = false;

    // Which side stepped last, so that when both players hold at once we
    // alternate into a real walk instead of starving one side.
    private bool lastStepLeft = false;

    // Jump state
    private bool grounded = false;
    private bool prevP1Jump = false;
    private bool prevP2Jump = false;
    private bool jumpPending = false;   // waiting out the buffer to see if the other player also jumps
    private float jumpPendingTimer = 0f;
    private bool jumpP1 = false;        // did P1 contribute to the pending jump
    private bool jumpP2 = false;        // did P2 contribute

    private void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (animator == null) animator = GetComponent<Animator>();
    }

    public void Tick(Vector2 p1Input, Vector2 p2Input, bool p1Jump, bool p2Jump)
    {
        HandleJump(p1Jump, p2Jump);
        HandleStepInput(p1Input, p2Input);
        AdvanceStep();
        DriveAnimator();
    }

    private void HandleStepInput(Vector2 p1Input, Vector2 p2Input)
    {
        // Level-triggered: while no step is running, any held direction starts
        // the next one. A tap yields a single step; holding auto-repeats one
        // step every stepDuration (the stepping lock paces it). Steps only start
        // on the ground so MovePosition never fights an airborne jump arc.
        if (stepping || !grounded)
            return;

        // Does each side want to act this frame? Forward/back = Y (either sign),
        // turn = X.
        bool leftWants  = Mathf.Abs(p1Input.y) >= inputThreshold || Mathf.Abs(p1Input.x) >= inputThreshold;
        bool rightWants = Mathf.Abs(p2Input.y) >= inputThreshold || Mathf.Abs(p2Input.x) >= inputThreshold;

        bool chooseLeft;
        if (leftWants && rightWants)
            chooseLeft = !lastStepLeft; // both held -> alternate into a walk
        else if (leftWants)
            chooseLeft = true;
        else if (rightWants)
            chooseLeft = false;
        else
            return; // nothing held

        // A step carries BOTH a forward/back component (Y) and a turn component
        // (X) at once, so pressing e.g. W+D takes one curved step. Either can be
        // zero; the chosen side always has at least one past threshold.
        if (chooseLeft)
            BeginStep(true, StepForward(p1Input.y), StepTurn(p1Input.x));
        else
            BeginStep(false, StepForward(p2Input.y), StepTurn(p2Input.x));

        lastStepLeft = chooseLeft;
    }

    private void HandleJump(bool p1Jump, bool p2Jump)
    {
        // Rising edges (pressed this frame)
        bool p1Edge = p1Jump && !prevP1Jump;
        bool p2Edge = p2Jump && !prevP2Jump;
        prevP1Jump = p1Jump;
        prevP2Jump = p2Jump;

        if (!jumpPending)
        {
            // Open a short buffer on the first press so a near-simultaneous
            // second press still counts as a coordinated (full) jump.
            if (grounded && (p1Edge || p2Edge))
            {
                jumpPending = true;
                jumpPendingTimer = jumpBufferTime;
                jumpP1 = p1Jump;
                jumpP2 = p2Jump;
            }
            else
            {
                return;
            }
        }
        else
        {
            // Catch the other player's press during the window
            jumpP1 |= p1Jump;
            jumpP2 |= p2Jump;
            jumpPendingTimer -= Time.deltaTime;
        }

        // Fire as soon as both are in, or when the buffer expires with just one.
        if ((jumpP1 && jumpP2) || jumpPendingTimer <= 0f)
        {
            ExecuteJump(jumpP1, jumpP2);
            jumpPending = false;
            jumpP1 = jumpP2 = false;
        }
    }

    private void ExecuteJump(bool p1, bool p2)
    {
        if (!grounded) return;

        // A jump interrupts any in-progress step so MovePosition stops pinning
        // the body while it leaves the ground.
        stepping = false;

        Vector3 v = rb.linearVelocity;

        if (p1 && p2)
        {
            v.y = jumpSpeed; // coordinated: full height, straight up
        }
        else
        {
            v.y = jumpSpeed * soloJumpFactor; // solo: lower...
            // ...and the arc leans toward the jumper instead of the body turning.
            // P2 owns the right side, P1 the left.
            float dir = p2 ? 1f : -1f;
            v += transform.right * (dir * soloJumpSideSpeed);
        }

        rb.linearVelocity = v;
        grounded = false;

        animator.Play(jumpStateName, 0, 0f);
        animator.Update(0f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag(groundTag)) grounded = true;
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.collider.CompareTag(groundTag)) grounded = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider.CompareTag(groundTag)) grounded = false;
    }

    // Y past threshold -> a forward (+) or backward (-) stride, else no advance.
    private float StepForward(float y)
    {
        return Mathf.Abs(y) >= inputThreshold ? Mathf.Sign(y) * strideDistance : 0f;
    }

    // X past threshold -> a turn right (+) or left (-), else no turn.
    private float StepTurn(float x)
    {
        return Mathf.Abs(x) >= inputThreshold ? Mathf.Sign(x) * turnStepAngle : 0f;
    }

    private void BeginStep(bool leftLeg, float forward, float turn)
    {
        stepping = true;
        everStepped = true;
        stepElapsed = 0f;
        // Each side owns a FIXED half of the walk cycle, so repeated taps of the
        // same side always reproduce that leg's step instead of walking on.
        // For this clip the first half [0, 0.5] is the right leg and the second
        // half [0.5, 1.0] is the left leg, so P1/W (leftLeg) takes the second
        // half. If the legs ever look swapped, flip these two values.
        gaitStart = leftLeg ? 0.5f : 0f;
        gaitEnd = gaitStart + 0.5f;
        // A backward step plays that same half in reverse so the leg steps back
        // instead of moonwalking.
        if (forward < 0f)
        {
            (gaitStart, gaitEnd) = (gaitEnd, gaitStart);
        }
        activeStepForward = forward;
        activeStepTurn = turn;
    }

    private void AdvanceStep()
    {
        if (!stepping)
            return;

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
            gaitTime = gaitEnd % 1f; // wrap cleanly
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
        if (!grounded)
            return; // airborne -> let the jump clip play instead of the walk scrub

        if (!stepping && !everStepped)
            return; // untouched -> character shows the controller's default idle

        animator.Play(walkStateName, 0, gaitTime % 1f);
        animator.Update(0f);
    }

    // Exposed so UI or debugging can show whether a step is in progress
    public bool IsStepping => stepping;
}
