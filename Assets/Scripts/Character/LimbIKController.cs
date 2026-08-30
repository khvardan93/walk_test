using UnityEngine;

public class LimbIKController : MonoBehaviour
{
    [Header("IK Targets")]
    [SerializeField] private Transform leftArmTarget;
    [SerializeField] private Transform leftLegTarget;
    [SerializeField] private Transform rightArmTarget;
    [SerializeField] private Transform rightLegTarget;

    [Header("Tuning")]
    [SerializeField] private float armReach = 0.25f;
    [SerializeField] private float legReach = 0.15f;
    [SerializeField] private float followSpeed = 12f;

    // Captured automatically from the actual bone positions
    private Vector3 leftArmRest, leftLegRest, rightArmRest, rightLegRest;

    private void Start()
    {
        // The auto-rig placed each target exactly on its bone, so its current
        // localPosition IS the correct rest pose. Capture it instead of guessing.
        leftArmRest  = leftArmTarget.localPosition;
        leftLegRest  = leftLegTarget.localPosition;
        rightArmRest = rightArmTarget.localPosition;
        rightLegRest = rightLegTarget.localPosition;
    }

    public void UpdateLimbs(Vector2 player1Input, Vector2 player2Input)
    {
        // Player 1 drives the left side
        Vector3 p1Arm = new Vector3(player1Input.x, player1Input.y, 0) * armReach;
        Vector3 p1Leg = new Vector3(player1Input.x, 0, player1Input.y) * legReach;

        // Player 2 drives the right side
        Vector3 p2Arm = new Vector3(player2Input.x, player2Input.y, 0) * armReach;
        Vector3 p2Leg = new Vector3(player2Input.x, 0, player2Input.y) * legReach;

        // All local space: targets stay parented to the character and follow it for free
        leftArmTarget.localPosition  = Vector3.Lerp(leftArmTarget.localPosition,  leftArmRest  + p1Arm, Time.deltaTime * followSpeed);
        leftLegTarget.localPosition  = Vector3.Lerp(leftLegTarget.localPosition,  leftLegRest  + p1Leg, Time.deltaTime * followSpeed);
        rightArmTarget.localPosition = Vector3.Lerp(rightArmTarget.localPosition, rightArmRest + p2Arm, Time.deltaTime * followSpeed);
        rightLegTarget.localPosition = Vector3.Lerp(rightLegTarget.localPosition, rightLegRest + p2Leg, Time.deltaTime * followSpeed);
    }
}