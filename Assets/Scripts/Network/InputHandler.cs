using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

public struct NetworkInputData : INetworkInput
{
    public Vector2 movement;
    public float rotation;
    public bool jumpPressed;
}

public class InputHandler : MonoBehaviour
{
    public static NetworkInputData CurrentInput { get; private set; }
    
    private Gamepad gamepad1;
    private Gamepad gamepad2;
    private Keyboard keyboard;
    
    private void Update()
    {
        Debug.Log("=== Input Update Called ==="); // DEBUG
        
        gamepad1 = Gamepad.all.Count > 0 ? Gamepad.all[0] : null;
        gamepad2 = Gamepad.all.Count > 1 ? Gamepad.all[1] : null;
        keyboard = Keyboard.current;
        
        var data = new NetworkInputData();
        
        // Player 1: Left Stick OR Keyboard (WASD)
        Vector2 p1_movement = gamepad1 != null ? gamepad1.leftStick.ReadValue() : Vector2.zero;
        
        if (keyboard != null && p1_movement.magnitude < 0.1f)
        {
            float kbX = (keyboard.dKey.isPressed ? 1f : 0f) - (keyboard.aKey.isPressed ? 1f : 0f);
            float kbY = (keyboard.wKey.isPressed ? 1f : 0f) - (keyboard.sKey.isPressed ? 1f : 0f);
            p1_movement = new Vector2(kbX, kbY);
        }
        
        // Player 2: Right Stick OR Arrow Keys
        Vector2 p2_movement = gamepad2 != null ? gamepad2.rightStick.ReadValue() : Vector2.zero;
        
        if (keyboard != null && p2_movement.magnitude < 0.1f)
        {
            float arrowX = (keyboard.rightArrowKey.isPressed ? 1f : 0f) - (keyboard.leftArrowKey.isPressed ? 1f : 0f);
            float arrowY = (keyboard.upArrowKey.isPressed ? 1f : 0f) - (keyboard.downArrowKey.isPressed ? 1f : 0f);
            p2_movement = new Vector2(arrowX, arrowY);
        }
        
        // COMBINE movement
        data.movement = p1_movement + p2_movement;
        if (data.movement.magnitude > 1f)
            data.movement = data.movement.normalized;
        
        // Jump
        bool spacePressed = keyboard != null && keyboard.spaceKey.wasPressedThisFrame;
        bool aButtonPressed = gamepad1 != null && gamepad1.aButton.wasPressedThisFrame;
        data.jumpPressed = spacePressed || aButtonPressed;
        
        // Store for NetworkPlayer to read
        CurrentInput = data;
    }
}