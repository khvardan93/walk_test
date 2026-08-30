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
        // Level-triggered: while no step is running, any held direction starts
        // the next one. A tap yields a single step; holding auto-repeats one
        // step every stepDuration (the stepping lock paces it).
        if (stepping)
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

        // Within the chosen side, a forward/back step beats a turn. The sign of
        // Y decides direction: +1 forward, -1 backward.
        if (chooseLeft)
        {
            if (Mathf.Abs(p1Input.y) >= inputThreshold) BeginStep(true, Mathf.Sign(p1Input.y) * strideDistance, 0f);
            else BeginStep(true, 0f, Mathf.Sign(p1Input.x) * turnStepAngle);
        }
        else
        {
            if (Mathf.Abs(p2Input.y) >= inputThreshold) BeginStep(false, Mathf.Sign(p2Input.y) * strideDistance, 0f);
            else BeginStep(false, 0f, Mathf.Sign(p2Input.x) * turnStepAngle);
        }

        lastStepLeft = chooseLeft;
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
        if (!stepping && !everStepped)
            return; // untouched -> character shows the controller's default idle

        animator.Play(walkStateName, 0, gaitTime % 1f);
        animator.Update(0f);
    }

    // Exposed so UI or debugging can show whether a step is in progress
    public bool IsStepping => stepping;
}
