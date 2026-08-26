using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

public class NetworkPlayer : MonoBehaviour, INetworkInput
{
    private CharacterMovement characterMovement;
    private NetworkInputData lastInputData;
    
    private void Start()
    {
        characterMovement = GetComponent<CharacterMovement>();
        
        if (characterMovement == null)
        {
            Debug.LogError("CharacterMovement not found!");
        }
    }
    
    private void Update()
    {
        // Get input from InputHandler
        lastInputData = InputHandler.CurrentInput;
        
        // Apply movement directly
        if (characterMovement != null)
        {
            characterMovement.UpdateMovement(lastInputData);
            Debug.Log("Update - Movement: " + lastInputData.movement);
        }
    }
    
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        // Store for network sync
        input.Set(lastInputData);
    }
}