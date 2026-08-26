using UnityEngine;
using System;

public static class GameEvents
{
    // Event fired when character is spawned and ready
    public static event Action<CharacterMovement> OnCharacterReady;
    
    // Event fired when player reaches finish
    public static event Action OnPlayerWon;
    
    // Event fired when time runs out
    public static event Action OnTimeUp;
    
    // Event fired when player falls off map
    public static event Action OnPlayerRespawn;
    
    // Call these methods to trigger events
    public static void CharacterReady(CharacterMovement character)
    {
        OnCharacterReady?.Invoke(character);
        Debug.Log("Event: Character Ready!");
    }
    
    public static void PlayerWon()
    {
        OnPlayerWon?.Invoke();
        Debug.Log("Event: Player Won!");
    }
    
    public static void TimeUp()
    {
        OnTimeUp?.Invoke();
        Debug.Log("Event: Time Up!");
    }
    
    public static void PlayerRespawn()
    {
        OnPlayerRespawn?.Invoke();
        Debug.Log("Event: Player Respawned!");
    }
}