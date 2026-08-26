using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class ObstacleCourse : MonoBehaviour
{
    [SerializeField] private Transform finishLine;
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI statusText;
    
    [SerializeField] private float timeLimit = 60f;
    [SerializeField] private float winDistance = 2f;
    
    private CharacterMovement playerCharacter;
    private float timeRemaining;
    private bool gameActive = false;
    private bool gameWon = false;
    
    private void Start()
    {
        timeRemaining = timeLimit;
        StartGame();
    
        // Subscribe to character ready event
        GameEvents.OnCharacterReady += OnCharacterReady;
    
        if (timerText == null)
        {
            Debug.LogError("Timer Text not assigned!");
        }
    
        if (statusText == null)
        {
            Debug.LogError("Status Text not assigned!");
        }
    }
    
    private void StartGame()
    {
        gameActive = true;
        gameWon = false;
        timeRemaining = timeLimit;
        
        if (statusText != null)
            statusText.text = "GO!";
    }
    
    private void Update()
    {
        if (!gameActive) return;
    
        // Update timer
        timeRemaining -= Time.deltaTime;
    
        if (timerText != null)
            timerText.text = "Time: " + Mathf.Max(0, timeRemaining).ToString("F1") + "s";
    
        // Check if time ran out
        if (timeRemaining <= 0)
        {
            LoseGame();
            return;
        }
    
        // Check if player reached finish line
        if (playerCharacter != null && finishLine != null)
        {
            float distToFinish = Vector3.Distance(playerCharacter.transform.position, finishLine.position);
        
            if (distToFinish < winDistance && !gameWon)
            {
                WinGame();
                return;
            }
        }
    
        // Check if player fell off the map
        if (playerCharacter != null)
        {
            if (playerCharacter.transform.position.y < -5f)
            {
                RespawnPlayer();
            }
        }
    
        // Restart with R key (NEW INPUT SYSTEM)
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard.rKey.wasPressedThisFrame)
        {
            RestartGame();
        }
    }
    
    private void WinGame()
    {
        gameActive = false;
        gameWon = true;
        
        if (statusText != null)
            statusText.text = "SUCCESS! Press R to restart";
        
        Time.timeScale = 0.5f; // Slow motion celebration
    }
    
    private void LoseGame()
    {
        gameActive = false;
        
        if (statusText != null)
            statusText.text = "TIME UP! Press R to restart";
    }
    
    private void RespawnPlayer()
    {
        if (playerCharacter != null && respawnPoint != null)
        {
            playerCharacter.transform.position = respawnPoint.position;
            
            Rigidbody rb = playerCharacter.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
    }
    
    private void RestartGame()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
    
    // Called when character is ready
    private void OnCharacterReady(CharacterMovement character)
    {
        playerCharacter = character;
        Debug.Log("ObstacleCourse: Character received!");
    }

// Clean up event subscription
    private void OnDestroy()
    {
        GameEvents.OnCharacterReady -= OnCharacterReady;
    }
}