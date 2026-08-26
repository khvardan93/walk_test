using Fusion;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform spawnPoint;
    
    private NetworkRunner runner;
    private bool playerSpawned = false;
    
    private async void Start()
    {
        await StartNetworkGame();
    }
    
    private async System.Threading.Tasks.Task StartNetworkGame()
    {
        // Create NetworkRunner GameObject
        var runnerGO = new GameObject("NetworkRunner");
        runner = runnerGO.AddComponent<NetworkRunner>();
        
        // Add InputHandler to the runner
        runnerGO.AddComponent<InputHandler>();
        
        // Configure runner
        var args = new StartGameArgs();
        args.GameMode = GameMode.Shared;
        args.SessionName = "CorpseParty";
        args.PlayerCount = 2;
        
        // Start the game (don't instantiate again!)
        await runner.StartGame(args);
        
        Debug.Log("Photon Fusion Started!");
    }
    
    private void Update()
    {
        // Spawn player when runner is ready
        if (runner != null && !playerSpawned && runner.IsRunning)
        {
            SpawnPlayer();
            playerSpawned = true;
        }
    }
    
    private void SpawnPlayer()
    {
        Vector3 pos = spawnPoint != null ? spawnPoint.position : Vector3.zero;
        Quaternion rot = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;
        
        // Spawn the character
        var playerInstance = Instantiate(playerPrefab, pos, rot);
        
        // Ensure it has CharacterMovement
        CharacterMovement charMovement = playerInstance.GetComponent<CharacterMovement>();
        if (charMovement == null)
            charMovement = playerInstance.AddComponent<CharacterMovement>();
        
        // Ensure it has NetworkPlayer
        NetworkPlayer netPlayer = playerInstance.GetComponent<NetworkPlayer>();
        if (netPlayer == null)
            netPlayer = playerInstance.AddComponent<NetworkPlayer>();
        
        Debug.Log("Player spawned at " + pos);
        
        // Fire event
        GameEvents.CharacterReady(charMovement);
    }
}