using UnityEngine;

public class SpinningObstacle : MonoBehaviour
{
    private float _spinSpeed = 3f;
    
    private void Update()
    {
        // Spin around Z axis
        transform.Rotate(0, 0, _spinSpeed);
    }
}