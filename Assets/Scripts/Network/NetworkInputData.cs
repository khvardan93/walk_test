using Fusion;
using UnityEngine;

public struct NetworkInputData : INetworkInput
{
    public Vector2 movement;        // Combined movement from both players
    public float rotation;          // Rotation input
    public bool jumpPressed;        // Jump input
}

public class InputHandler : MonoBehaviour, INetworkInput
{
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        var data = new NetworkInputData();
        
        // Player 1: WASD or Left Stick
        float p1_horizontal = Input.GetAxis("Horizontal");      // A/D or Left Stick X
        float p1_vertical = Input.GetAxis("Vertical");          // W/S or Left Stick Y
        
        // Player 2: Arrow Keys or Right Stick (we'll set these up in Input Manager)
        float p2_horizontal = Input.GetAxis("Horizontal2");     // Arrow Keys or Right Stick X
        float p2_vertical = Input.GetAxis("Vertical2");         // Arrow Keys or Right Stick Y
        
        // COMBINE movement from both players
        // This is the chaos! Both players push the character in different directions
        data.movement = new Vector2(
            p1_horizontal + p2_horizontal,
            p1_vertical + p2_vertical
        );
        
        // Clamp to -1 to 1
        if (data.movement.magnitude > 1f)
            data.movement = data.movement.normalized;
        
        // Rotation (who controls it? Let's say both can rotate, left trigger vs right trigger)
        float rotLeft = Input.GetAxis("RotateLeft");   // LT
        float rotRight = Input.GetAxis("RotateRight"); // RT
        data.rotation = rotRight - rotLeft;
        
        // Jump (either player can jump with Space or button)
        data.jumpPressed = Input.GetButtonDown("Jump");
        
        input.Set(data);
    }
}