using UnityEngine;

public class SimpleFollowCamera : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0, 2.5f, -4f);
    [SerializeField] private float followSpeed = 5f;
    [SerializeField] private bool lookAtTarget = true;
    
    private void LateUpdate()
    {
        if (target == null)
        {
            // Try to auto-find spawned character
            var player = GameObject.FindWithTag("Player");
            if (player != null) target = player.transform;
            return;
        }
        
        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * followSpeed);
        
        if (lookAtTarget)
        {
            Vector3 lookPoint = target.position + Vector3.up * 1f;
            transform.LookAt(lookPoint);
        }
    }
    
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}