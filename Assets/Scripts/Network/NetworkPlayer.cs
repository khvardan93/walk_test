using Fusion;
using UnityEngine;

public class NetworkPlayer : NetworkBehaviour, INetworkInput
{
    private CharacterMovement _characterMovement;
    private Rigidbody _rb;
    private NetworkInputData _lastInputData;
    
    private void Start()
    {
        _characterMovement = GetComponent<CharacterMovement>();
        _rb = GetComponent<Rigidbody>();
        
        if (_characterMovement == null)
        {
            Debug.LogError("CharacterMovement script not found on " + gameObject.name);
        }
    }
    
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        var data = new NetworkInputData();
        
        // Player 1: Left Stick (Horizontal, Vertical)
        float p1_x = Input.GetAxis("Horizontal");
        float p1_y = Input.GetAxis("Vertical");
        
        // Player 2: Right Stick (RightStickX, RightStickY)
        float p2_x = Input.GetAxis("RightStickX");
        float p2_y = Input.GetAxis("RightStickY");
        
        // COMBINE movement from both players
        // This creates the chaos - both try to push character in different directions
        data.movement = new Vector2(
            p1_x + p2_x,
            p1_y + p2_y
        );
        
        // Clamp to prevent over-acceleration
        if (data.movement.magnitude > 1f)
            data.movement = data.movement.normalized;
        
        // Jump: Either player can jump with Space
        data.jumpPressed = Input.GetButtonDown("Jump");
        
        _lastInputData = data;
        input.Set(data);
    }
    
    public override void FixedUpdateNetwork()
    {
        // Only update movement on the input authority (the player controlling this)
        if (HasInputAuthority && _characterMovement != null)
        {
            _characterMovement.UpdateMovement(_lastInputData);
        }
    }
}