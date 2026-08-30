using UnityEngine;

public class NetworkPlayer : MonoBehaviour
{
    private CharacterMovement characterMovement;
    
    private void Start()
    {
        characterMovement = GetComponent<CharacterMovement>();
    }
    
    private void Update()
    {
        if (characterMovement != null)
        {
            characterMovement.UpdateMovement(InputHandler.Player1Input, InputHandler.Player2Input, InputHandler.JumpPressed);
        }
    }
}