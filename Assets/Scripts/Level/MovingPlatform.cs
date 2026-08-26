using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    private float _startX;
    private float _moveSpeed = 2f;
    private float _moveDistance = 1.5f;
    
    private void Start()
    {
        _startX = transform.position.x;
    }
    
    private void Update()
    {
        // Sway left and right
        float newX = _startX + Mathf.Sin(Time.time * _moveSpeed) * _moveDistance;
        transform.position = new Vector3(newX, transform.position.y, transform.position.z);
    }
}