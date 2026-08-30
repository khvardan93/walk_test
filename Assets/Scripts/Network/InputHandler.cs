using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    public static Vector2 Player1Input { get; private set; } // WASD
    public static Vector2 Player2Input { get; private set; } // Arrow Keys
    public static bool JumpPressed { get; private set; }
    
    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;
        
        // Player 1: WASD
        float p1x = (keyboard.dKey.isPressed ? 1f : 0f) - (keyboard.aKey.isPressed ? 1f : 0f);
        float p1y = (keyboard.wKey.isPressed ? 1f : 0f) - (keyboard.sKey.isPressed ? 1f : 0f);
        Player1Input = new Vector2(p1x, p1y);
        
        // Player 2: Arrow Keys
        float p2x = (keyboard.rightArrowKey.isPressed ? 1f : 0f) - (keyboard.leftArrowKey.isPressed ? 1f : 0f);
        float p2y = (keyboard.upArrowKey.isPressed ? 1f : 0f) - (keyboard.downArrowKey.isPressed ? 1f : 0f);
        Player2Input = new Vector2(p2x, p2y);
        
        JumpPressed = keyboard.spaceKey.wasPressedThisFrame;
    }
}