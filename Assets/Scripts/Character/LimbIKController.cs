using UnityEngine;

public class LimbIKController : MonoBehaviour
{
    [Header("IK Targets")]
    [SerializeField] private Transform leftArmTarget;
    [SerializeField] private Transform leftLegTarget;
    [SerializeField] private Transform rightArmTarget;
    [SerializeField] private Transform rightLegTarget;
    
    [Header("Rest Positions (relative to character)")]
    [SerializeField] private Vector3 leftArmRest = new Vector3(-0.3f, 1.0f, 0.2f);
    [SerializeField] private Vector3 leftLegRest = new Vector3(-0.2f, 0f, 0.2f);
    [SerializeField] private Vector3 rightArmRest = new Vector3(0.3f, 1.0f, 0.2f);
    [SerializeField] private Vector3 rightLegRest = new Vector3(0.2f, 0f, 0.2f);
    
    [SerializeField] private float reach = 0.4f;
    [SerializeField] private float followSpeed = 10f;
    
    private Vector3 leftArmGoal, leftLegGoal, rightArmGoal, rightLegGoal;
    
    private void Start()
    {
        leftArmGoal = leftArmTarget.position;
        leftLegGoal = leftLegTarget.position;
        rightArmGoal = rightArmTarget.position;
        rightLegGoal = rightLegTarget.position;
    }
    
    // Called every frame with the latest input
    public void UpdateLimbs(Vector2 player1Input, Vector2 player2Input)
    {
        Vector3 basePos = transform.position;
        
        // Player 1 controls LEFT arm + leg
        Vector3 p1Offset = new Vector3(player1Input.x, 0, player1Input.y) * reach;
        leftArmGoal = basePos + leftArmRest + p1Offset;
        leftLegGoal = basePos + leftLegRest + p1Offset;
        
        // Player 2 controls RIGHT arm + leg
        Vector3 p2Offset = new Vector3(player2Input.x, 0, player2Input.y) * reach;
        rightArmGoal = basePos + rightArmRest + p2Offset;
        rightLegGoal = basePos + rightLegRest + p2Offset;
        
        // Smoothly move targets toward goals
        leftArmTarget.position = Vector3.Lerp(leftArmTarget.position, leftArmGoal, Time.deltaTime * followSpeed);
        leftLegTarget.position = Vector3.Lerp(leftLegTarget.position, leftLegGoal, Time.deltaTime * followSpeed);
        rightArmTarget.position = Vector3.Lerp(rightArmTarget.position, rightArmGoal, Time.deltaTime * followSpeed);
        rightLegTarget.position = Vector3.Lerp(rightLegTarget.position, rightLegGoal, Time.deltaTime * followSpeed);
    }
}